using System.Collections.Generic;

namespace SecureBot
{
    public class QuizManager
    {
        public List<QuizQuestion> Questions { get; private set; }
        public int CurrentQuestionIndex { get; set; }
        public int Score { get; set; }
        public bool IsInQuiz { get; set; }
        public bool ResumeQuiz { get; set; }

        public QuizManager()
        {
            InitializeQuiz();
        }

        private void InitializeQuiz()
        {
            Questions = new List<QuizQuestion>
{
    new QuizQuestion {
        Question = "What should you do if you receive an email asking for your password?",
        Options = new[] { "A) Reply with your password", "B) Delete the email", "C) Report the email as phishing", "D) Ignore it" },
        CorrectOptionIndex = 2,
        Explanation = "Correct! Reporting phishing emails helps prevent scams."
    },
    new QuizQuestion {
        Question = "True or False: You should use the same password for multiple accounts.",
        Options = new[] { "True", "False" },
        CorrectOptionIndex = 1,
        Explanation = "False! Reusing passwords increases your risk if one gets exposed."
    },
    new QuizQuestion {
        Question = "Which of these is a strong password?",
        Options = new[] { "A) Password123", "B) mybirthday", "C) Q!9v@eR#3k", "D) admin123" },
        CorrectOptionIndex = 2,
        Explanation = "Q!9v@eR#3k uses symbols, numbers, and is harder to guess."
    },
    new QuizQuestion {
        Question = "What does a VPN do?",
        Options = new[] { "A) Blocks pop-ups", "B) Encrypts internet traffic", "C) Deletes your history", "D) Speeds up internet" },
        CorrectOptionIndex = 1,
        Explanation = "A VPN encrypts your internet traffic and protects your privacy."
    },
    new QuizQuestion {
        Question = "True or False: Legitimate companies ask for your password via email.",
        Options = new[] { "True", "False" },
        CorrectOptionIndex = 1,
        Explanation = "False! No legitimate company will ask for your password via email."
    },
    new QuizQuestion {
        Question = "What should you look for in a secure website?",
        Options = new[] { "A) HTTPS in the URL", "B) Lots of ads", "C) Flash animations", "D) All caps text" },
        CorrectOptionIndex = 0,
        Explanation = "HTTPS ensures your data is encrypted when sent to the site."
    },
    new QuizQuestion {
        Question = "True or False: Antivirus software should be updated regularly.",
        Options = new[] { "True", "False" },
        CorrectOptionIndex = 0,
        Explanation = "True! Updates ensure protection against the latest threats."
    },
    new QuizQuestion {
        Question = "What is phishing?",
        Options = new[] { "A) Fishing with tech", "B) A hacking technique", "C) A scam to steal info", "D) Email marketing" },
        CorrectOptionIndex = 2,
        Explanation = "Phishing scams trick you into revealing personal information."
    },
    new QuizQuestion {
        Question = "What’s a safe practice on public Wi-Fi?",
        Options = new[] { "A) Use banking apps", "B) Shop online", "C) Use a VPN", "D) Access work servers" },
        CorrectOptionIndex = 2,
        Explanation = "Use a VPN to protect your data on public Wi-Fi."
    },
    new QuizQuestion {
        Question = "True or False: You should clear cookies regularly.",
        Options = new[] { "True", "False" },
        CorrectOptionIndex = 0,
        Explanation = "True! This limits tracking and protects your privacy."
    }
};
        }

        public void ResetQuiz()
        {
            CurrentQuestionIndex = 0;
            Score = 0;
            IsInQuiz = true;
            ResumeQuiz = false;
        }
        public int ParseAnswer(string input, QuizQuestion question)
        {
            if (int.TryParse(input, out int selectedIndex) && selectedIndex > 0 && selectedIndex <= question.Options.Length)
            {
                return selectedIndex - 1;
            }
            return -1;
        }
    }
}

