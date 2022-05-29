using Abundance_Nk.Model.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Abundance_Nk.Web.Api.DTO
{
    public class CourseContentDto
    {
        public long Id { get; set; }
        public string Url { get; set; }
        public string VideoUrl { get; set; }
        public string LiveStreamingLink { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }

    }
    public class ChatModel
    {
        public string StudentName { get; set; }
        public string MatricNumber { get; set; }
        public string CourseTitle { get; set; }
        public string CourseCode { get; set; }
        public List<EChatBoard> EChatBoards { get; set; }


    }

}