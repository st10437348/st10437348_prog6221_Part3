using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace SecureBot
{
    public partial class MainWindow : Window
    {
        private string userName = "";
        private bool isNameAsked = false;
        private bool isMoodAsked = false;
        private Random rand = new Random();

        private bool isTaskMenu = false;
        private bool isQuizMenu = false;

        private Dictionary<string, List<string>> keywordResponses;
        private Dictionary<string, string> sentiments;

        private TaskManager taskManager = new TaskManager();
        private QuizManager quizManager = new QuizManager();

        private bool isAddingTask = false;
        private bool isDeletingTask = false;
        private TaskItem currentTask = null;

        private enum TaskStep { None, Title, Description, AskReminder, WaitForReminder }
        private TaskStep taskStep = TaskStep.None;

        private List<string> activityLog = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeBot();
        }

        private static readonly SemaphoreSlim botMessageSemaphore = new SemaphoreSlim(1, 1);

        private async Task PrintBotMessage(string message)
        {
            await botMessageSemaphore.WaitAsync();

            try
            {
                TextBlock botMessage = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(5),
                    FontSize = 16
                };

                Run botNameRun = new Run("SecureBot: ")
                {
                    Foreground = Brushes.LightGreen
                };

                Run messageRun = new Run("")
                {
                    Foreground = Brushes.White
                };

                botMessage.Inlines.Add(botNameRun);
                botMessage.Inlines.Add(messageRun);

                ChatPanel.Children.Add(botMessage);
                ChatScrollViewer.ScrollToEnd();

                foreach (char c in message)
                {
                    messageRun.Text += c;
                    await Task.Delay(30);
                    ChatScrollViewer.ScrollToEnd();
                }
            }
            finally
            {
                botMessageSemaphore.Release();
            }
        }

        private void PrintUserMessage(string message)
        {
            TextBlock userMessage = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(5),
                FontSize = 16,
            };

            Run userNameRun = new Run(userName + ": ")
            {
                Foreground = Brushes.LightBlue
            };

            Run messageRun = new Run(message)
            {
                Foreground = Brushes.White
            };

            userMessage.Inlines.Add(userNameRun);
            userMessage.Inlines.Add(messageRun);

            ChatPanel.Children.Add(userMessage);
            ChatScrollViewer.ScrollToEnd();
        }

        private async void InitializeBot()
        {
            keywordResponses = new Dictionary<string, List<string>>()
    {
        { "passwords", new List<string> {
            "Strong passwords should be at least 12 characters long, using a combination of uppercase and lowercase letters, numbers and symbols to enhance complexity. For example {0}123@IieMsa.",
            "Avoid using obvious choices like your name, birthdate, or common words, as these can be easily guessed or cracked by attackers.",
            "Each account should have a unique password; reusing passwords increases the risk of a domino effect if one account is compromised.",
            "A password manager helps generate, store, and autofill complex passwords securely, reducing the temptation to use weak or repeated passwords.",
            "Change your passwords regularly, especially for sensitive accounts, and immediately after any suspected security breach."
        }},
        { "scams", new List<string> {
            "Scammers often impersonate trusted organizations or people—always confirm identities through official contact channels before responding.",
            "Watch out for urgent requests for money or personal data; creating panic is a common tactic used to override your better judgment.",
            "Do not trust caller ID or email headers at face value—these can be spoofed to appear legitimate while being fraudulent.",
            "Take a moment to research any suspicious offers or requests online. Many scams follow common patterns that others have reported.",
            "Reporting scams to authorities and platforms can help prevent others from falling victim and contributes to broader cybersecurity efforts."
        }},
        { "privacy", new List<string> {
            "Think carefully about what you share online; even casual posts can reveal information useful to identity thieves or scammers.",
            "Adjust your privacy settings on social media and mobile apps to limit access to your personal data and location.",
            "Uninstall apps you no longer use and routinely review permissions to ensure you’re not oversharing data with unnecessary services.",
            "Consider using privacy-focused browsers and search engines, like Brave or DuckDuckGo, to minimize tracking and data collection.",
            "Use alias emails and phone numbers for sign-ups and mailing lists to reduce exposure and better manage who can contact you."
        }},
        { "phishing", new List<string> {
            "Phishing emails often create urgency before acting on a message asking for sensitive information or money.",
            "Carefully examine email addresses and URLs; subtle differences or misspellings can signal a phishing attempt.",
            "Avoid clicking on links or downloading files in unexpected messages, even if they appear to come from someone you know.",
            "Legitimate companies will never ask you for your password or payment details via email—when in doubt, contact them directly.",
            "Use spam filters and report phishing emails so they can be blocked in the future and help protect others."
        }},
        { "malware", new List<string> {
            "Install antivirus software from a reputable provider and keep it updated to protect against known and emerging threats.",
            "Be cautious with files downloaded from unfamiliar websites or shared via email—they could carry malicious payloads.",
            "Keep your operating system, browsers and plugins up to date; many malware infections exploit outdated software.",
            "Avoid pirated software or media, as they are common sources of bundled malware and other unwanted programs.",
            "Back up your data regularly to secure locations, such as encrypted cloud storage or external drives, in case of a malware attack like ransomware."
        }},
        { "vpn", new List<string> {
            "A VPN encrypts your internet traffic, which protects your data from hackers on unsecured public networks like coffee shop Wi-Fi.",
            "Select a trustworthy VPN provider that doesn’t log your activity and offers strong encryption and fast, stable connections.",
            "VPNs help prevent websites, advertisers, and even your ISP from tracking your browsing habits across the web.",
            "Some countries restrict content based on location; a VPN allows you to access these resources securely while traveling.",
            "Always turn on your VPN before accessing sensitive services or when connecting to networks you don’t control."
        }},
        { "safe browsing", new List<string> {
            "Look for a padlock icon and 'https://' in the URL bar before entering sensitive information on a website.",
            "Install browser extensions that enhance security and privacy, such as ad blockers or anti-tracking tools.",
            "Avoid clicking suspicious links, even if they appear on familiar websites—malvertising can target trusted platforms.",
            "Log out of accounts after use and clear cookies regularly to minimize exposure from session hijacking or tracking.",
            "Only download software from verified, official sources to avoid inadvertently installing malicious code."
        }},
        { "identity theft", new List<string> {
            "Monitor your credit reports and bank statements regularly for any signs of unauthorized activity or new account openings.",
            "Don’t share personal information like your Social Security number, home address, or full birthdate unless absolutely necessary.",
            "Use multifactor authentication wherever possible—this adds a critical layer of defense even if your password is compromised.",
            "Avoid using public Wi-Fi for sensitive tasks like banking unless you’re connected through a secure VPN.",
            "Shred physical documents containing personal details before disposal to prevent dumpster diving data theft."
        }},
        { "encryption", new List<string> {
            "Encrypt important files, especially those stored on portable devices or cloud services, to prevent unauthorized access if lost or stolen.",
            "Use communication platforms that support end-to-end encryption so your messages can’t be read by anyone except the intended recipient.",
            "Encryption is only as secure as your passphrase—use long, complex phrases and avoid reusing them across services.",
            "Enable device encryption on laptops, smartphones, and tablets so that even if your device is stolen, your data remains protected.",
            "Be aware of where your encrypted data is stored and make sure the keys or passwords needed to unlock it are stored securely."
        }},
        { "firewalls", new List<string> {
            "Firewalls monitor and control incoming and outgoing traffic to protect against unauthorized access and malicious activity.",
            "Use both hardware (on your router) and software (on your device) firewalls to create multiple layers of defense.",
            "Review and customize your firewall rules to limit what applications can communicate over the internet.",
            "Keep your firewall software updated to ensure it's equipped to recognize and respond to the latest threats.",
            "Avoid disabling your firewall for convenience—if necessary, only disable temporarily and re-enable immediately after the task is complete."
        }},
    };

            sentiments = new Dictionary<string, string>() {
        { "worried", "It's normal to feel worried {0}. Cybersecurity can seem complex, but I'm here to simplify it for you." },
        { "curious", "Curiosity is the first step towards being cyber-aware. Let's explore together, {0}!" },
        { "frustrated", "Don't worry {0}, we'll take it one step at a time. You're not alone in this." },
        { "scared", "No need to fear {0}. Knowledge is your best defense." },
        { "confused", "Let's clear up that confusion together. Ask me anything thats on your mind, {0}." },
        { "overwhelmed", "Take a deep breath {0}. We'll tackle each topic at your pace." },
        { "anxious", "Anxiety is understandable. I'm here to make things easier for you." },
        { "happy", "That's wonderful, {0}! Positive energy helps us learn better." },
        { "bored", "I'm sorry {0}. I'll do my best to keep our chat engaging and informative!" },
        { "relaxed", "Great stuff, {0}! A relaxed mind is perfect for learning." }
    };

            await PrintBotMessage("Hello! I'm SecureBot, your personal guide to navigating the world of cybersecurity with confidence and clarity. I am looking forward to chatting with you and assisting in any way I can.");
            await PrintBotMessage("Before we begin, what is your name? I would love to address you in a more personal manner since you already know my name.");
        }

        private void AddToActivityLog(string actionDescription)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            activityLog.Add($"{timestamp} - {actionDescription}");

            if (activityLog.Count > 10)
            {
                activityLog.RemoveAt(0);
            }
        }

        private async void TasksButton_Click(object sender, RoutedEventArgs e)
        {
            isTaskMenu = true;
            await PrintBotMessage(
                "\n--Tasks Menu--\n" +
                "1) Add a task\n" +
                "2) Show tasks\n" +
                "3) Delete a task\n" +
                "Please type 1, 2, or 3."
            );
        }

        private async void QuizButton_Click(object sender, RoutedEventArgs e)
        {
            isQuizMenu = true;
            await PrintBotMessage(
                "\n--Quiz Menu--\n" +
                "1) Take quiz\n" +
                "2) Show quiz results\n" +
                "Please type 1 or 2."
            );
        }

        private void ShowActivityLogButton_Click(object sender, RoutedEventArgs e)
        {
            ShowActivityLog();
        }

        private async void StartQuiz(object sender, RoutedEventArgs e)
        {
            if (quizManager.IsInQuiz)
            {
                await PrintBotMessage("You're already in a quiz session.");
                return;
            }

            if (quizManager.CurrentQuestionIndex > 0 && quizManager.CurrentQuestionIndex < quizManager.Questions.Count)
            {
                await PrintBotMessage("You have an unfinished quiz. Would you like to 'resume' or 'start over'?");
                quizManager.ResumeQuiz = true;
            }
            else
            {
                quizManager.ResetQuiz();
                AddToActivityLog("Quiz started.");
                AskNextQuizQuestion();
            }
        }

        private async void AskNextQuizQuestion()
        {
            if (quizManager.CurrentQuestionIndex >= quizManager.Questions.Count)
            {
                await PrintBotMessage($"Quiz complete! Your score: {quizManager.Score}/{quizManager.Questions.Count}.");

                if (quizManager.Score >= 8)
                    await PrintBotMessage("Great job! You're a cybersecurity pro!");
                else if (quizManager.Score >= 5)
                    await PrintBotMessage("Nice effort! You're on the right path.");
                else
                    await PrintBotMessage("Keep learning to stay safe online!");

                AddToActivityLog($"Quiz completed with score {quizManager.Score}/{quizManager.Questions.Count}.");

                quizManager.IsInQuiz = false;
                return;
            }

            var q = quizManager.Questions[quizManager.CurrentQuestionIndex];
            string options = string.Join("\n", q.Options);
            await PrintBotMessage($"Question {quizManager.CurrentQuestionIndex + 1}: {q.Question}\n{options}");
        }

        private async void HandleQuizAnswer(string input)
        {
            var q = quizManager.Questions[quizManager.CurrentQuestionIndex];
            int selectedIndex = -1;

            if (int.TryParse(input, out int num))
            {
                selectedIndex = num - 1;
            }
            else if (input.Length == 1 && char.IsLetter(input[0]))
            {
                selectedIndex = char.ToUpper(input[0]) - 'A';
            }
            else if (input.ToLower() == "true") selectedIndex = 0;
            else if (input.ToLower() == "false") selectedIndex = 1;

            if (selectedIndex == q.CorrectOptionIndex)
            {
                await PrintBotMessage("✅ Correct! " + q.Explanation);
                quizManager.Score++;
                AddToActivityLog($"Quiz question {quizManager.CurrentQuestionIndex + 1} answered correctly.");
            }
            else
            {
                await PrintBotMessage("❌ Incorrect. " + q.Explanation);
                AddToActivityLog($"Quiz question {quizManager.CurrentQuestionIndex + 1} answered incorrectly.");
            }

            quizManager.CurrentQuestionIndex++;
            AskNextQuizQuestion();
        }


        private async void ShowQuizResults(object sender, RoutedEventArgs e)
        {
            if (quizManager.CurrentQuestionIndex == 0)
            {
                await PrintBotMessage("You haven't started the quiz yet.");
                return;
            }

            await PrintBotMessage($"Quiz progress: You answered {quizManager.CurrentQuestionIndex} of {quizManager.Questions.Count} questions.");
            await PrintBotMessage($"Current score: {quizManager.Score}/{quizManager.Questions.Count}");

            if (!quizManager.IsInQuiz && quizManager.CurrentQuestionIndex < quizManager.Questions.Count)
            {
                await PrintBotMessage("Please click the 'Quiz' button and then type '1' to resume or start over.");
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e) => HandleUserInput();

        private void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                HandleUserInput();
            }
        }

        private async void ShowActivityLog()
        {
            if (activityLog.Count == 0)
            {
                await PrintBotMessage("No recent activities to show.");
            }
            else
            {
                await PrintBotMessage("Here's your recent activity log:");
                foreach (var entry in activityLog)
                {
                    await PrintBotMessage(entry);
                }
            }

            AddToActivityLog("Displayed activity log to user.");
        }

        private async void HandleUserInput()
        {
            string input = UserInputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            PrintUserMessage(input);
            UserInputTextBox.Clear();

            if (quizManager.ResumeQuiz)
            {
                if (input.ToLower().Contains("resume"))
                {
                    quizManager.IsInQuiz = true;
                    quizManager.ResumeQuiz = false;
                    AskNextQuizQuestion();
                    return;
                }
                else if (input.ToLower().Contains("start"))
                {
                    quizManager.ResetQuiz();
                    AskNextQuizQuestion();
                    return;
                }
                else
                {
                    await PrintBotMessage("Please type 'resume' or 'start over'.");
                    return;
                }
            }

            if (quizManager.IsInQuiz)
            {
                if (input.ToLower() == "leave quiz")
                {
                    await PrintBotMessage($"Quiz paused. You've answered {quizManager.CurrentQuestionIndex} of {quizManager.Questions.Count} questions.");
                    quizManager.IsInQuiz = false;
                    return;
                }

                HandleQuizAnswer(input);
                return;
            }

            if (isAddingTask)
            {
                HandleTaskConversation(input);
                return;
            }

            if (isDeletingTask)
            {
                if (int.TryParse(input, out int index) && index >= 1 && index <= taskManager.Tasks.Count)
                {
                    string removedTitle = taskManager.Tasks[index - 1].Title;
                    taskManager.DeleteTask(index - 1);
                    await PrintBotMessage($"Task \"{removedTitle}\" has been deleted.");
                    AddToActivityLog($"Task deleted: '{removedTitle}'.");
                }
                else
                {
                    await PrintBotMessage("Invalid input. Please enter a valid task number.");
                }
                isDeletingTask = false;
                return;
            }

            if (isTaskMenu)
            {
                switch (input)
                {
                    case "1":
                        AddTasks(null, null);
                        AddToActivityLog("User requested to add a task via the button.");
                        break;
                    case "2":
                        ShowTasks(null, null);
                        AddToActivityLog("User requested to show tasks via the button.");
                        break;
                    case "3":
                        DeleteTasks(null, null);
                        AddToActivityLog("User requested to delete a task via the button.");
                        break;
                    default:
                        await PrintBotMessage("Please enter 1, 2, or 3.");
                        return;
                }
                isTaskMenu = false;
                return;
            }

            if (isQuizMenu)
            {
                switch (input)
                {
                    case "1":
                        StartQuiz(null, null);
                        AddToActivityLog("User requested to start a quiz via the button.");
                        break;
                    case "2":
                        ShowQuizResults(null, null);
                        AddToActivityLog("User requested to show results of the quiz via the button.");
                        break;
                    default:
                        await PrintBotMessage("Please enter 1 or 2.");
                        return;
                }
                isQuizMenu = false;
                return;
            }
            await Task.Run(() => Dispatcher.Invoke(() => ProcessBotResponse(input.ToLower())));
        }

        private async void ProcessBotResponse(string input)
        {
            if (!isNameAsked)
            {
                userName = input.Length > 0
                    ? char.ToUpper(input[0]) + input.Substring(1)
                    : input;
                isNameAsked = true;
                await PrintBotMessage($"It's a pleasure to meet you, {userName}. Together, we'll explore how to stay safe and smart online and hopefully by the end of our conversation you would have learnt a thing or two.");
                await PrintBotMessage("How are you feeling today?");
                return;
            }

            if (!isMoodAsked)
            {
                isMoodAsked = true;
                if (input.Contains("good") || input.Contains("great") || input.Contains("fine") || input.Contains("well") || input.Contains("okay"))
                {
                    await PrintBotMessage("That's fantastic! A positive mindset is a great shield against cyber threats.");
                }
                else
                {
                    await PrintBotMessage("I'm sorry to hear that. Perhaps learning a few security tips will brighten your day.");
                }

                await PrintBotMessage("If you are unsure what I can do for you, ask me what I know or what I can do. ");
                return;
            }

            string lowerInput = input.ToLower();

            if (lowerInput.Contains("bye") || lowerInput.Contains("goodbye") || lowerInput.Contains("exit") || lowerInput.Contains("quit"))
            {
                await PrintBotMessage($"Goodbye {userName}! Stay safe and remember to come back if you have any more questions.");
                return;
            }

            if ((lowerInput.Contains("add") || lowerInput.Contains("create")) &&
                (lowerInput.Contains("task") || lowerInput.Contains("tasks")))
            {
                AddToActivityLog("User requested to add a task via chat command.");
                AddTasks(null, null);
                return;
            }

            if ((lowerInput.Contains("show") || lowerInput.Contains("list") || lowerInput.Contains("view")) &&
                (lowerInput.Contains("task") || lowerInput.Contains("tasks")))
            {
                AddToActivityLog("User requested to show tasks via chat command.");
                ShowTasks(null, null);
                return;
            }

            if ((lowerInput.Contains("delete") || lowerInput.Contains("remove")) &&
                (lowerInput.Contains("task") || lowerInput.Contains("tasks")))
            {
                AddToActivityLog("User requested to delete a task via chat command.");
                DeleteTasks(null, null);
                return;
            }

            if ((lowerInput.Contains("take") || lowerInput.Contains("start") || lowerInput.Contains("begin")) &&
                (lowerInput.Contains("quiz")))
            {
                AddToActivityLog("User requested to start a quiz via chat command.");
                StartQuiz(null, null);
                return;
            }

            if ((lowerInput.Contains("show") || lowerInput.Contains("view") || lowerInput.Contains("see")) &&
                (lowerInput.Contains("quiz") && lowerInput.Contains("result")))
            {
                AddToActivityLog("User requested to show quiz results via chat command.");
                ShowQuizResults(null, null);
                return;
            }

            if ((lowerInput.Contains("show") || lowerInput.Contains("view") || lowerInput.Contains("see")) &&
                (lowerInput.Contains("activity") && lowerInput.Contains("log") ||
                 lowerInput.Contains("what") && lowerInput.Contains("have") && lowerInput.Contains("done")))
            {
                ShowActivityLog();
                return;
            }

            if (lowerInput.Contains("how are you"))
            {
                await PrintBotMessage($"I'm up and running as smoothly as ever, thanks for asking {userName}!");
                return;
            }

            if (lowerInput.Contains("what") &&
                (lowerInput.Contains("ask") || lowerInput.Contains("know")))
            {
                await PrintBotMessage("You can ask me about: \n-passwords \n-scams \n-privacy \n-phishing \n-malware \n-VPN \n-safe browsing \n-identity theft \n-encryption \n-firewalls");
                return;
            }
            if (lowerInput.Contains("what") && lowerInput.Contains("do"))
            {
                await PrintBotMessage("I can help you learn about cybersecurity, answer your questions, guide you through adding or managing tasks, taking quizzes and more.");
                return;
            }

            bool matched = false;

            foreach (var sentiment in sentiments)
            {
                if (input.Contains(sentiment.Key))
                {
                    await PrintBotMessage(string.Format(sentiment.Value, userName));
                    matched = true;
                    break;
                }
            }

            foreach (var keyword in keywordResponses)
            {
                if (input.Contains(keyword.Key))
                {
                    string response = keyword.Value[rand.Next(keyword.Value.Count)];
                    response = response.Contains("{0}") ? string.Format(response, userName) : response;
                    await PrintBotMessage(response);
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                await PrintBotMessage($"I'm not sure I understand that. Could you maybe try rephrasing your response {userName}? I will do my best to interpret your response");
            }
        }

        private async void HandleTaskConversation(string input)
        {
            switch (taskStep)
            {
                case TaskStep.Title:
                    currentTask = new TaskItem { Title = input };
                    await PrintBotMessage("Please enter a description for the task.");
                    taskStep = TaskStep.Description;
                    break;

                case TaskStep.Description:
                    currentTask.Description = input;
                    await PrintBotMessage("Would you like a reminder? (yes/no)");
                    taskStep = TaskStep.AskReminder;
                    break;

                case TaskStep.AskReminder:
                    if (input.ToLower().StartsWith("y"))
                    {
                        await PrintBotMessage("Please set the reminder date in format dd/mm/yyyy:");
                        taskStep = TaskStep.WaitForReminder;
                    }
                    else
                    {
                        currentTask.ReminderDate = "";
                        taskManager.AddTask(currentTask);
                        await PrintBotMessage("Task added with no reminder.");
                        AddToActivityLog($"Task added: '{currentTask.Title}' with no reminder.");
                        isAddingTask = false;
                        taskStep = TaskStep.None;
                    }
                    break;

                case TaskStep.WaitForReminder:
                    currentTask.ReminderDate = input;
                    taskManager.AddTask(currentTask);
                    await PrintBotMessage($"Got it. I'll remind you on {input}.");
                    AddToActivityLog($"Task added: '{currentTask.Title}' with reminder set for {input}.");
                    isAddingTask = false;
                    taskStep = TaskStep.None;
                    break;
            }
        }

        private async void AddTasks(object sender, RoutedEventArgs e)
        {
            isAddingTask = true;
            taskStep = TaskStep.Title;
            await PrintBotMessage("Let's add a new task! What is the title of your task?");
        }

        private async void ShowTasks(object sender, RoutedEventArgs e)
        {
            if (taskManager.Tasks.Count == 0)
            {
                await PrintBotMessage("You currently have no tasks.");
                return;
            }

            await PrintBotMessage("Here are your current tasks:\n");
            for (int i = 0; i < taskManager.Tasks.Count; i++)
            {
                var task = taskManager.Tasks[i];
                string reminder = string.IsNullOrWhiteSpace(task.ReminderDate) ? "No reminder" : $"Reminder: {task.ReminderDate}";
                await PrintBotMessage($"{i + 1}. {task.Title}\nDescription: {task.Description}\n{reminder}");
            }
        }

        private async void DeleteTasks(object sender, RoutedEventArgs e)
        {
            if (taskManager.Tasks.Count == 0)
            {
                await PrintBotMessage("There are no tasks to delete.");
                return;
            }

            string prompt = "Enter the number of the task you want to delete:\n";
            for (int i = 0; i < taskManager.Tasks.Count; i++)
                prompt += $"{i + 1}. {taskManager.Tasks[i].Title}\n";

            await PrintBotMessage(prompt);
            isDeletingTask = true;
        }
    }
}


