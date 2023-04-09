using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Walto
{
    public class Program
    {
        private static readonly string _cardPath = "cards.json";
        private static List<Card>? allCards;

        private static readonly string logoText = "█░░░█ █▀▀█ █░░ ▀▀█▀▀ █▀▀█\n█▄█▄█ █▄▄█ █░░ ░░█░░ █░░█\n░▀░▀░ ▀░░▀ ▀▀▀ ░░▀░░ ▀▀▀▀";
        private static readonly string[] Doors = { "Balcony", "Office", "Student" };

        private static int currentMenuId = 0;

        public static void Main(string[] args)
        {
            if (!File.Exists(_cardPath))
            {
                using (StreamWriter sw = File.CreateText(_cardPath))
                {
                    sw.Write("[]");
                }
            }

            string json = File.ReadAllText(_cardPath);
            allCards = JsonConvert.DeserializeObject<List<Card>>(json);

            Thread ESCKeyThread = new(EscKeyThread);
            ESCKeyThread.Start();

            MainMenu();
        }

        private static void MainMenu()
        {
            currentMenuId = 0;

            Console.Clear();
            Console.WriteLine(logoText);
            Console.WriteLine("Enter 1 To Create A Card");
            Console.WriteLine("Enter 2 To View A Card");
            Console.WriteLine("Enter 3 To View All Cards");
            Console.WriteLine("Enter 4 To View All Doors");
            Console.WriteLine("Press [ESC] to Exit");

            int option;
            while (!int.TryParse(Console.ReadLine(), out option) || option < 1 || option > 3)
            {
                Console.Write("Invalid Option. Please choose correctly: ");
            }

            switch (option)
            {
                case 1:
                    CardCreationMenu();
                    break;
                case 2:
                    ViewCard();
                    break;
                case 3:
                    ViewAllCards();
                    break;
            }

            Console.ReadKey();
        }

        private static void EscKeyThread()
        {
            while (true)
            {
                if (KeyPressed(0x1B))
                {
                    if (currentMenuId != 0)
                    {
                        MainMenu();
                    }   
                    else
                    {
                        Environment.Exit(0);
                    }
                }
            }
        }

        private static void CardCreationMenu()
        {
            currentMenuId = 1;

            Console.Clear();
            Console.WriteLine(logoText);
            Console.WriteLine("- Walto Card Creation -");
            Console.WriteLine("Press [ESC] to go Back");
            Console.Write("Enter the user's full name: ");

            // Input and Validate user name
            string userName;
            Regex regex = new("^[a-zA-Z ]+$");

            while (true)
            {
                Console.Write("Enter your name: ");
                userName = Console.ReadLine();

                if (regex.IsMatch(userName))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid Name. Please enter a name with only letters and spaces.");
                }
            }

            // Input and Validate user id
            Console.Write("Enter the user's ID (7 digits): ");
            int userId;

            while (true)
            {
                string input = Console.ReadLine();
                if (input.Length == 7 && int.TryParse(input, out userId))
                {
                    break;
                }
                else
                {
                    Console.Write("Invalid ID Format. Please enter a 7 digit number: ");
                }
            }

            // Give student area doors by default
            Console.WriteLine("Enter the user's permissions in a comma seperated list");
            Console.WriteLine("- `Student` permission is applied by default");
            Console.WriteLine("- Leave blank to skip this step");
            Console.WriteLine("` Valid Permissions: " + string.Join(", ", Doors));

            string[] perms = { "Student" };

            while (true)
            {
                Console.Write("Enter permissions: ");
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    break;
                }
                string[] inputItems = input.Split(',');
                bool valid = true;
                foreach (string item in inputItems)
                {
                    string trimmedItem = item.Trim();
                    if (!Doors.Contains(trimmedItem))
                    {
                        valid = false;
                        break;
                    }
                    if (!perms.Contains(trimmedItem))
                    {
                        Array.Resize(ref perms, perms.Length + 1);
                        perms[^1] = trimmedItem;
                    }
                }

                if (valid)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a comma-separated list of valid doors.");
                    Console.WriteLine("Valid Permissions: " + string.Join(", ", Doors));
                }
            }
            // Input and validate Valid From Date
            DateTime CreationStamp;
            while (true)
            {
                Console.Write("Enter the user's valid from date (dd/mm/yyyy): ");
                string input = Console.ReadLine();
                if (DateTime.TryParseExact(input, "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out CreationStamp))
                {
                    if (CreationStamp.Date >= DateTime.Today)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid Input. The entered date is in the past. Please enter a valid date.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid Input. Please enter a valid date in the format dd/mm/yyyy.");
                }
            }
            // Input and validate Valid Till Date
            DateTime ExpiryStamp;
            while (true)
            {
                Console.Write("Enter the user's valid till date (dd/mm/yyyy): ");
                string input = Console.ReadLine();
                if (DateTime.TryParseExact(input, "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ExpiryStamp))
                {
                    if (ExpiryStamp >= CreationStamp)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid Input. The date entered is before the valid from date.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid Input. Please enter a valid date in the format dd/mm/yyyy.");
                }
            }
            Console.WriteLine("Great! All details have been entered, please check below:");
            Console.WriteLine("User Name: " + userName);
            Console.WriteLine("User ID: " + userId.ToString());
            Console.WriteLine("User Permissions: " + string.Join(", ", perms));
            Console.WriteLine("User Valid From: " + CreationStamp.ToString("d/M/yyyy"));
            Console.WriteLine("User Valid Till: " + ExpiryStamp.ToString("d/M/yyyy"));
            Console.WriteLine("Press Enter to proceed otherwise press ESC");
            // Detect Enter or ESC key
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey();

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    createCard(userName, userId, perms, CreationStamp, ExpiryStamp);
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    MainMenu();
                }

            } while (keyInfo.Key is not ConsoleKey.Enter and not ConsoleKey.Escape);
        }

        private static void createCard(string userName, int userId, string[] perms, DateTime CreationStamp, DateTime ExpiryStamp)
        {
            // Create the card object and store it in json file
            string cardId = generateCardId();
            Card newCard = new(cardId, userName, userId, perms, CreationStamp, ExpiryStamp);

            // Add to json file
            JObject existingCards;
            if (File.Exists("cards.json"))
            {
                string json = File.ReadAllText("cards.json");
                existingCards = JsonConvert.DeserializeObject<JObject>(json);
            }
            else
            {
                existingCards = new JObject();
            }

            // Add the new card to the existing cards object
            existingCards[cardId] = JObject.FromObject(newCard);

            // Write the updated object back to the JSON file with formatting
            string jsonString = JsonConvert.SerializeObject(existingCards, Formatting.Indented);
            File.WriteAllText("cards.json", jsonString);
        }

        private static string generateCardId()
        {
            Random random = new();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string cardId = new(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
            return cardId;
        }

        private static void ViewCard()
        {
            currentMenuId = 2;
            Console.Clear();
            Console.WriteLine(logoText);
            Console.WriteLine("- Walto Card View -");
            Console.WriteLine("Press [ESC] to go Back");
            string json = File.ReadAllText("cards.json");
            Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            while (true)
            {
                Console.Write("Enter a card id to view: ");
                string searchKey = Console.ReadLine();

                if (!data.ContainsKey(searchKey))
                {
                    Console.WriteLine("Card ID `{0}` not found.", searchKey);
                }
                else
                {
                    // The key was found, so we can access its data
                    dynamic cardData = data[searchKey];
                    Console.WriteLine("Card ID: {0}", cardData.cardId);
                    Console.WriteLine("User Name: {0}", cardData.userName);
                    Console.WriteLine("User ID: {0}", cardData.userId);
                    Console.WriteLine("Permissions: {0}", string.Join(", ", cardData.perms));
                    Console.WriteLine("Valid From Date: {0}", cardData.CreationStamp.ToString("d/M/yyyy"));
                    Console.WriteLine("Valid Till Date: {0}", cardData.ExpiryStamp.ToString("d/M/yyyy"));
                    break;
                }
            }
        }

        private static void ViewAllCards()
        {
            currentMenuId = 3;
            Console.Clear();
            Console.WriteLine(logoText);
            Console.WriteLine("- Walto Card View -");
            Console.WriteLine("Press [ESC] to go Back");
            Console.WriteLine("Below are all the current cards:");
            Console.WriteLine("------------------------------");
            foreach (Card card in allCards)
            {
                if (card != null)
                {
                    Console.WriteLine("Card ID: " + card.Id);
                    Console.WriteLine("Card User Name: " + card.UserName);
                    Console.WriteLine("Card User ID: " + card.UserId);
                    if (card.Permissions != null)
                    {
                        Console.WriteLine("Permissions: " + string.Join(", ", card.Permissions));
                    }
                    else
                    {
                        Console.WriteLine("Permissions is null.");
                    }
                    Console.WriteLine("Creation Stamp: " + card.CreationStamp);
                    Console.WriteLine("Expiry Stamp: " + card.ExpiryStamp);
                    Console.WriteLine("------------------------------");
                }
            }
        }


        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        public static bool KeyPressed(int vk)
        {
            return (GetAsyncKeyState(vk) & 32768) != 0;
        }
    }
}