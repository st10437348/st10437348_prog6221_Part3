using System.Collections.Generic;

namespace SecureBot
{
    public class QuizQuestion
    {
        public string Question { get; set; }
        public string[] Options { get; set; }
        public int CorrectOptionIndex { get; set; }
        public string Explanation { get; set; }
    }
}

