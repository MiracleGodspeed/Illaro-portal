using Abundance_Nk.Model.Entity;
using Abundance_Nk.Model.Entity.Model;
using Abundance_Nk.Model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abundance_Nk.Model.Translator
{
    public class FeedbackStoreTranslator : TranslatorBase<FeedbackStore, FEEDBACK_STORE>
    {
        private LiveLecturesTranslator liveLecturesTranslator;
        public FeedbackStoreTranslator()
        {
            liveLecturesTranslator = new LiveLecturesTranslator();
        }
        public override FeedbackStore TranslateToModel(FEEDBACK_STORE entity)
        {
            try
            {
                FeedbackStore model = null;
                if (entity != null)
                {
                    model = new FeedbackStore();
                    model.Id = entity.Id;
                    model.LiveLectures = liveLecturesTranslator.Translate(entity.LIVE_LECTURES);
                    model.Comments = entity.Comments;
                    model.Active = entity.Active;

                }

                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override FEEDBACK_STORE TranslateToEntity(FeedbackStore model)
        {
            try
            {
                FEEDBACK_STORE entity = null;
                if (model != null)
                {
                    entity = new FEEDBACK_STORE();
                    entity.Id = model.Id;
                    entity.Live_Lecture_Id = model.LiveLectures.Id;
                    entity.Comments = model.Comments;
                    entity.Active = model.Active;
                    

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
