using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Model.Translator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Business
{
    public class LiveLecturesLogic : BusinessBaseLogic<LiveLectures, LIVE_LECTURES>
    {
        public LiveLecturesLogic()
        {
            translator = new LiveLecturesTranslator();
        }


        public bool Modify(LiveLectures liveLectures)
        {
            try
            {
                Expression<Func<LIVE_LECTURES, bool>> selector = f => f.Id == liveLectures.Id;
                LIVE_LECTURES entity = GetEntityBy(selector);
                if (entity != null && entity.Session_Id > 0)
                {
                    if (liveLectures.Topic != null)
                    {
                        entity.Topic = liveLectures.Topic;
                    }
                    if (liveLectures.Agenda != null)
                    {
                        entity.Agenda = liveLectures.Agenda;
                    }
                    if (liveLectures.Duration > 0)
                    {
                        entity.Duration = liveLectures.Duration;
                    }
                    if (liveLectures.Time > 0)
                    {
                        entity.Time = liveLectures.Time;
                    }
                    if (liveLectures.LectureDate != null)
                    {
                        entity.Lecture_Date = liveLectures.LectureDate;
                    }


                    int modifiedRecordCount = Save();

                    if (modifiedRecordCount > 0)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
