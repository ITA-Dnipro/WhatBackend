﻿using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CharlieBackend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using CharlieBackend.Core.DTO.Lesson;
using CharlieBackend.Data.Repositories.Impl.Interfaces;
using CharlieBackend.Core;
using System;
using CharlieBackend.Data.Exceptions;

namespace CharlieBackend.Data.Repositories.Impl
{
    public class LessonRepository : Repository<Lesson>, ILessonRepository
    {
        public LessonRepository(ApplicationContext applicationContext) 
            : base(applicationContext) 
        { 
        }

        public new Task<List<Lesson>> GetAllAsync()
        {
            return _applicationContext.Lessons
                    .Include(lesson => lesson.Visits)
                    .Include(lesson => lesson.Theme)
                    .ToListAsync();
        }

        public async Task<List<Lesson>> GetAllLessonsForMentor(long mentorId)
        {
            return await _applicationContext.Lessons
                .Where(lesson => lesson.MentorId == mentorId)
                .Select(lesson => lesson)
                .ToListAsync();
        }

        public async Task<Lesson> GetLessonByHomeworkId(long homeworkId)
        {
            var homework = await _applicationContext.Homeworks.FirstOrDefaultAsync(x => x.Id == homeworkId);

            return await _applicationContext.Lessons
                .FirstOrDefaultAsync(l => l.Id == homework.LessonId);
        }

        public async Task<List<Lesson>> GetLessonsForMentorAsync(long? studentGroupId, DateTime? startDate, DateTime? finishDate, long mentorId)
        {
            return await _applicationContext.Lessons
                .Where(x=> x.MentorId == mentorId)
                .WhereIf(studentGroupId != default, x => x.StudentGroupId == studentGroupId)
                .WhereIf((startDate != default) && (finishDate != default), x => (startDate < x.LessonDate) && (x.LessonDate < finishDate))
                .ToListAsync();
        }

        public async Task<List<Lesson>> GetLessonsForStudentAsync(long? studentGroupId, DateTime? startDate, DateTime? finishDate, long studentId)
        {
            return await _applicationContext.StudentsOfStudentGroups
                .Where(
                    studentsOfGroup => studentsOfGroup.StudentId == studentId &&
                    studentsOfGroup.StudentGroupId == studentGroupId
                )
                .Join(
                    _applicationContext.Lessons,
                    studentOfGroup => studentOfGroup.StudentGroupId,
                    l => l.StudentGroupId,
                    (students_of_student_group, lesson) => lesson
                )
                .Where(lesson=> (lesson.LessonDate > startDate) && (lesson.LessonDate < finishDate))
                .Include(lesson => lesson.Theme)
                .ToListAsync();
        }

        public async Task<IList<StudentLessonDto>> GetStudentInfoAsync(long studentId)
        {
            try
            {
                var studentLessonDtos = new List<StudentLessonDto>();

                var visits = await _applicationContext.Visits
                        .Include(visit => visit.Lesson)
                        .ThenInclude(lesson => lesson.Theme)
                        .Where(visit => visit.StudentId == studentId).ToListAsync();

                for (int i = 0; i < visits.Count; i++)
                {
                    var studentLessonDto = new StudentLessonDto
                    {
                        Id = visits[i].Lesson.Id,
                        Comment = visits[i].Comment,
                        Mark = visits[i].StudentMark,
                        Presence = visits[i].Presence,
                        ThemeName = visits[i].Lesson.Theme.Name,
                        LessonDate = visits[i].Lesson.LessonDate,
                        StudentGroupId = visits[i].Lesson.StudentGroupId
                    };

                    studentLessonDtos.Add(studentLessonDto);
                }

                return studentLessonDtos;
            }
            catch 
            {
                return null; 
            }
        }

        public async override Task<Lesson> GetByIdAsync(long id)
        {
            return await _applicationContext.Lessons
                .Include(x => x.Visits)
                .FirstOrDefaultAsync(entity => entity.Id == id);
        }

        public async Task<Visit> GetVisitByStudentHomeworkIdAsync(long studentHomeworkId)
        {
            Visit visit = null;
            try
            {
                visit = await _applicationContext.Visits.
                    FirstAsync(visit => visit.Lesson.Homeworks
                    .Any(hw => hw.HomeworkStudents
                    .Any(hws => hws.Id == studentHomeworkId)));
            }
            catch
            {
                throw new NotFoundException($"Student howework with id {studentHomeworkId} not found");
            }

            return visit;
        }
    }
}
