using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Translator
{
    public class LiveLecturesAttendanceTranslator : TranslatorBase<LiveLecturesAttendance, LIVE_LECTURE_ATTENDANCE>
    {
        private LiveLecturesTranslator liveLecturesTranslator;
        private StudentTranslator studentTranslator;
        private CourseRegistrationDetailTranslator courseRegistrationDetailTranslator;
        public LiveLecturesAttendanceTranslator()
        {
            liveLecturesTranslator = new LiveLecturesTranslator();
            studentTranslator = new StudentTranslator();
            courseRegistrationDetailTranslator = new CourseRegistrationDetailTranslator();
        }

        public override LiveLecturesAttendance TranslateToModel(LIVE_LECTURE_ATTENDANCE entity)
        {
            try
            {
                LiveLecturesAttendance model = null;
                if (entity != null)
                {
                    model = new LiveLecturesAttendance();
                    model.Id = entity.Id;
                    model.LiveLectures = liveLecturesTranslator.Translate(entity.LIVE_LECTURES);
                    model.CourseRegistrationDetail = courseRegistrationDetailTranslator.Translate(entity.STUDENT_COURSE_REGISTRATION_DETAIL);
                    model.Student = studentTranslator.Translate(entity.STUDENT);
                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override LIVE_LECTURE_ATTENDANCE TranslateToEntity(LiveLecturesAttendance model)
        {
            try
            {
                LIVE_LECTURE_ATTENDANCE entity = null;
                if (model != null)
                {
                    entity = new LIVE_LECTURE_ATTENDANCE();
                    entity.Id = entity.Id;
                    entity.Live_Lecture_Id = model.LiveLectures.Id;
                    entity.Student_Id = model.Student.Id;
                    entity.Student_Course_Registration_Detail_Id = model.CourseRegistrationDetail.Id;

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
