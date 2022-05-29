using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Translator
{
    public class LiveLecturesTranslator : TranslatorBase<LiveLectures, LIVE_LECTURES>
    {
        private ProgrammeTranslator programmeTranslator;
        private DepartmentTranslator departmentTranslator;
        private SessionTranslator sessionTranslator;
        private LevelTranslator levelTranslator;
        private CourseAllocationTranslator courseAllocationTranslator;
        public LiveLecturesTranslator()
        {
            programmeTranslator = new ProgrammeTranslator();
            courseAllocationTranslator = new CourseAllocationTranslator();
            levelTranslator = new LevelTranslator();
            departmentTranslator = new DepartmentTranslator();
            sessionTranslator = new SessionTranslator();
        }

        public override LiveLectures TranslateToModel(LIVE_LECTURES entity)
        {
            try
            {
                LiveLectures model = null;
                if (entity != null)
                {
                    model = new LiveLectures();
                    model.Id = entity.Id;
                    model.Programme = programmeTranslator.Translate(entity.PROGRAMME);                   
                    model.Department = departmentTranslator.Translate(entity.DEPARTMENT);
                    model.Session = sessionTranslator.Translate(entity.SESSION);
                    model.CourseAllocation = courseAllocationTranslator.Translate(entity.COURSE_ALLOCATION);
                    model.Level = levelTranslator.Translate(entity.LEVEL);
                    model.Topic = entity.Topic;
                    model.Agenda = entity.Agenda;
                    model.Join_Meeting_Url = entity.Join_Meeting_Url;
                    model.Start_Meeting_Url = entity.Start_Meeting_Url;
                    model.LectureDate = entity.Lecture_Date;
                    model.DateCreated = entity.Date_Created;
                    model.Duration = entity.Duration;
                    model.Time = entity.Time;
                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override LIVE_LECTURES TranslateToEntity(LiveLectures model)
        {
            try
            {
                LIVE_LECTURES entity = null;
                if (model != null)
                {
                    entity = new LIVE_LECTURES();
                    entity.Id = entity.Id;
                    entity.Programme_Id = model.Programme.Id;
                    entity.Department_Id = model.Department.Id;
                    entity.Session_Id = model.Session.Id;
                    if(model.Level != null)
                    {
                        entity.Level_Id = model.Level.Id;
                    }
                    entity.Topic = model.Topic;
                    entity.Start_Meeting_Url = model.Start_Meeting_Url;
                    entity.Join_Meeting_Url = model.Join_Meeting_Url;
                    entity.Duration = model.Duration;
                    entity.Time = model.Time;
                    entity.Lecture_Date = model.LectureDate;
                    entity.Date_Created = model.DateCreated;
                    entity.Agenda = model.Agenda;
                    entity.Course_Allocation_Id = model.CourseAllocation.Id;

                }
                return entity;

            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
