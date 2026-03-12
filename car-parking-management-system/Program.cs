using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace CarParkingManagementSystem
{
    #region Main Program Class
    class Program
    {
        // --- Data Stores ---
        // These lists hold the live data for the application session.
        static List<User> users;
        static List<ParkingSlot> slots;
        static List<ParkingRecord> parkingLedger; // Stores all records, past and present.

        // --- Configuration ---
        const decimal FOUR_WHEELER_HOURLY_RATE = 5.0m;
        const decimal TWO_WHEELER_HOURLY_RATE = 2.0m;
        static User loggedInUser = null;

        static void Main(string[] args)
        {
            Console.Title = "Car Parking Management System (v2.0)";
            DataManager.LoadAllData(out users, out slots, out parkingLedger);

            while (true)
            {
                if (loggedInUser == null)
                {
                    Login();
                }
                else
                {
                    ShowMainMenu();
                }
            }
        }

        #region Authentication
        static void Login()
        {
            Console.Clear();
            UIHelper.DrawHeader("System Login");
            Console.Write("Enter Username: ");
            string username = Console.ReadLine();
            Console.Write("Enter Password: ");
            string password = UIHelper.ReadPassword();

            loggedInUser = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.Password == password);

            if (loggedInUser != null)
            {
                UIHelper.PrintSuccess($"\nWelcome, {loggedInUser.FullName} ({loggedInUser.Role})!");
                Thread.Sleep(1500);
            }
            else
            {
                UIHelper.PrintError("\nInvalid username or password.");
                Thread.Sleep(1500);
            }
        }

        static void Logout()
        {
            Console.WriteLine("\nLogging out...");
            loggedInUser = null;
            Thread.Sleep(1000);
        }
        #endregion

        #region Main Menu & UI
        static void ShowMainMenu()
        {
            Console.Clear();
            UIHelper.DrawHeader($"Main Dashboard | Welcome, {loggedInUser.FullName}");
            DisplaySlotStatus();

            var menu = new Dictionary<string, string>();
            if (loggedInUser.Role == UserRole.Admin || loggedInUser.Role == UserRole.Attendant)
            {
                menu.Add("1", "Vehicle Entry");
                menu.Add("2", "Vehicle Exit & Billing");
            }
            if (loggedInUser.Role == UserRole.Admin)
            {
                menu.Add("3", "System Reports");
                menu.Add("4", "Admin Functions");
            }
            menu.Add("9", "Logout");
            menu.Add("0", "Save & Exit");

            UIHelper.DrawMenu("Main Menu", menu);
            Console.Write("\nEnter your choice: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": VehicleEntry(); break;
                case "2": VehicleExit(); break;
                case "3": if (loggedInUser.Role == UserRole.Admin) ReportManager.ShowReportMenu(parkingLedger); else UIHelper.ShowAccessDenied(); break;
                case "4": if (loggedInUser.Role == UserRole.Admin) AdminManager.ShowAdminMenu(users, slots, parkingLedger); else UIHelper.ShowAccessDenied(); break;
                case "9": Logout(); break;
                case "0":
                    DataManager.SaveAllData(users, slots, parkingLedger);
                    UIHelper.PrintSuccess("All data saved. Exiting application.");
                    Thread.Sleep(1000);
                    Environment.Exit(0);
                    break;
                default:
                    UIHelper.PrintError("Invalid choice. Please try again.");
                    UIHelper.Pause();
                    break;
            }
        }

        static void DisplaySlotStatus()
        {
            int fourWheelerTotal = slots.Count(s => s.Type == VehicleType.FourWheeler);
            int twoWheelerTotal = slots.Count(s => s.Type == VehicleType.TwoWheeler);

            int fourWheelerOccupied = parkingLedger.Count(p => p.ExitTime == null && slots.Any(s => s.SlotNumber == p.AllocatedSlotNumber && s.Type == VehicleType.FourWheeler));
            int twoWheelerOccupied = parkingLedger.Count(p => p.ExitTime == null && slots.Any(s => s.SlotNumber == p.AllocatedSlotNumber && s.Type == VehicleType.TwoWheeler));

            Console.WriteLine("--- Parking Availability ---");
            Console.WriteLine($"4-Wheeler: {fourWheelerTotal - fourWheelerOccupied} Free / {fourWheelerTotal} Total");
            Console.WriteLine($"2-Wheeler: {twoWheelerTotal - twoWheelerOccupied} Free / {twoWheelerTotal} Total");
            Console.WriteLine("----------------------------");
        }
        #endregion

        #region Core Modules
        static void VehicleEntry()
        {
            Console.Clear();
            UIHelper.DrawHeader("New Vehicle Entry");
            try
            {
                Console.Write("Enter Vehicle Number: ");
                string vehicleNumber = Console.ReadLine().ToUpper();
                if (string.IsNullOrWhiteSpace(vehicleNumber)) throw new Exception("Vehicle Number cannot be empty.");

                if (parkingLedger.Any(v => v.VehicleNumber.Equals(vehicleNumber, StringComparison.OrdinalIgnoreCase) && v.ExitTime == null))
                {
                    throw new Exception("This vehicle is already parked.");
                }

                Console.Write("Enter Owner's Name: ");
                string ownerName = Console.ReadLine();

                Console.Write("Select Vehicle Type (1 for 4-Wheeler, 2 for 2-Wheeler): ");
                string typeChoice = Console.ReadLine();
                VehicleType type;
                if (typeChoice == "1") type = VehicleType.FourWheeler;
                else if (typeChoice == "2") type = VehicleType.TwoWheeler;
                else throw new Exception("Invalid vehicle type selected.");

                var occupiedSlots = parkingLedger.Where(p => p.ExitTime == null).Select(p => p.AllocatedSlotNumber);
                ParkingSlot allocatedSlot = slots.FirstOrDefault(s => s.Type == type && !occupiedSlots.Contains(s.SlotNumber));

                if (allocatedSlot == null)
                {
                    throw new Exception($"No free slots available for {type}.");
                }

                var newRecord = new ParkingRecord(vehicleNumber, ownerName, type, allocatedSlot.SlotNumber, DateTime.Now);
                parkingLedger.Add(newRecord);
                DataManager.SaveParkingLedger(parkingLedger); // Save immediately

                UIHelper.PrintSuccess("\n--- Entry Ticket ---");
                Console.WriteLine($"Ticket Number: {newRecord.TicketNumber}");
                Console.WriteLine($"Owner: {newRecord.OwnerName}");
                Console.WriteLine($"Vehicle Number: {newRecord.VehicleNumber}");
                Console.WriteLine($"Slot Number: {newRecord.AllocatedSlotNumber}");
                Console.WriteLine($"Entry Time: {newRecord.EntryTime:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine("--------------------");
            }
            catch (Exception ex)
            {
                UIHelper.PrintError($"\nError: {ex.Message}");
            }
            UIHelper.Pause();
        }

        static void VehicleExit()
        {
            Console.Clear();
            UIHelper.DrawHeader("Vehicle Exit & Billing");
            try
            {
                Console.Write("Enter Vehicle Number to exit: ");
                string vehicleNumber = Console.ReadLine().ToUpper();

                ParkingRecord vehicleToExit = parkingLedger.FirstOrDefault(v => v.VehicleNumber.Equals(vehicleNumber, StringComparison.OrdinalIgnoreCase) && v.ExitTime == null);

                if (vehicleToExit == null)
                {
                    throw new Exception("Vehicle not found in the parking lot.");
                }

                DateTime exitTime = DateTime.Now;
                TimeSpan duration = exitTime - vehicleToExit.EntryTime;
                decimal hourlyRate = vehicleToExit.Type == VehicleType.FourWheeler ? FOUR_WHEELER_HOURLY_RATE : TWO_WHEELER_HOURLY_RATE;
                decimal totalFee = (decimal)Math.Ceiling(duration.TotalHours > 0 ? duration.TotalHours : 1) * hourlyRate;

                UIHelper.PrintInfo("\n--- Parking Bill ---");
                Console.WriteLine($"Vehicle Number: {vehicleToExit.VehicleNumber}");
                Console.WriteLine($"Owner: {vehicleToExit.OwnerName}");
                Console.WriteLine($"Entry Time: {vehicleToExit.EntryTime:g}");
                Console.WriteLine($"Exit Time:  {exitTime:g}");
                Console.WriteLine($"Duration:   {duration.Hours} hours, {duration.Minutes} minutes");
                Console.WriteLine($"Hourly Rate: ${hourlyRate}");
                Console.WriteLine("--------------------");
                Console.WriteLine($"Total Fee:  ${totalFee:F2}");
                Console.WriteLine("--------------------");

                Console.Write("\nConfirm payment and exit? (y/n): ");
                if (Console.ReadLine().ToLower() == "y")
                {
                    vehicleToExit.ExitTime = exitTime;
                    vehicleToExit.TotalFee = totalFee;
                    DataManager.SaveParkingLedger(parkingLedger); // Save updated record

                    UIHelper.PrintSuccess("\nVehicle exited successfully. Slot is now free.");
                }
                else
                {
                    Console.WriteLine("\nExit cancelled.");
                }
            }
            catch (Exception ex)
            {
                UIHelper.PrintError($"\nError: {ex.Message}");
            }
            UIHelper.Pause();
        }
        #endregion
    }
    #endregion

    #region Entity Classes
    public enum UserRole { Admin, Attendant, Security }
    public enum VehicleType { TwoWheeler, FourWheeler, EV, VIP }

    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public UserRole Role { get; set; }
    }

    public class ParkingSlot
    {
        public string SlotNumber { get; set; }
        public VehicleType Type { get; set; }
    }

    public class ParkingRecord
    {
        public string TicketNumber { get; set; }
        public string VehicleNumber { get; set; }
        public string OwnerName { get; set; }
        public VehicleType Type { get; set; }
        public string AllocatedSlotNumber { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public decimal? TotalFee { get; set; }

        public ParkingRecord() { } // For CSV parsing
        public ParkingRecord(string vehicleNumber, string ownerName, VehicleType type, string slotNumber, DateTime entryTime)
        {
            TicketNumber = "T" + DateTime.Now.ToString("yyMMddHHmmssfff");
            VehicleNumber = vehicleNumber;
            OwnerName = ownerName;
            Type = type;
            AllocatedSlotNumber = slotNumber;
            EntryTime = entryTime;
        }
    }
    #endregion

    #region DataManager (File I/O)
    public static class DataManager
    {
        private const string UsersFile = "users.csv";
        private const string SlotsFile = "slots.csv";
        private const string LedgerFile = "parking_ledger.csv";

        public static void LoadAllData(out List<User> users, out List<ParkingSlot> slots, out List<ParkingRecord> ledger)
        {
            users = LoadData(UsersFile, ParseUser, GetDefaultUsers);
            slots = LoadData(SlotsFile, ParseSlot, GetDefaultSlots);
            ledger = LoadData(LedgerFile, ParseRecord, () => new List<ParkingRecord>());
        }

        public static void SaveAllData(List<User> users, List<ParkingSlot> slots, List<ParkingRecord> ledger)
        {
            SaveData(UsersFile, users, u => $"{u.Username},{u.Password},{u.FullName},{(int)u.Role}");
            SaveData(SlotsFile, slots, s => $"{s.SlotNumber},{(int)s.Type}");
            SaveParkingLedger(ledger);
        }

        public static void SaveParkingLedger(List<ParkingRecord> ledger)
        {
            SaveData(LedgerFile, ledger, r =>
                $"{r.TicketNumber},{r.VehicleNumber},{r.OwnerName},{(int)r.Type},{r.AllocatedSlotNumber},{r.EntryTime:O},{r.ExitTime:O},{r.TotalFee}");
        }

        private static List<T> LoadData<T>(string fileName, Func<string[], T> parser, Func<List<T>> defaultDataProvider)
        {
            if (!File.Exists(fileName))
            {
                return defaultDataProvider();
            }
            var data = new List<T>();
            try
            {
                var lines = File.ReadAllLines(fileName);
                foreach (var line in lines.Skip(1)) // Skip header
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var values = line.Split(',');
                    if (values.Length > 0 && !string.IsNullOrWhiteSpace(values[0]))
                    {
                        data.Add(parser(values));
                    }
                }
            }
            catch (Exception ex)
            {
                UIHelper.PrintError($"Error loading {fileName}: {ex.Message}");
                UIHelper.Pause();
                return defaultDataProvider(); // Return default data on error
            }
            return data;
        }

        private static void SaveData<T>(string fileName, List<T> data, Func<T, string> formatter)
        {
            var lines = new List<string>();
            // Add header based on type
            if (typeof(T) == typeof(User)) lines.Add("Username,Password,FullName,Role");
            else if (typeof(T) == typeof(ParkingSlot)) lines.Add("SlotNumber,Type");
            else if (typeof(T) == typeof(ParkingRecord)) lines.Add("TicketNumber,VehicleNumber,OwnerName,Type,AllocatedSlotNumber,EntryTime,ExitTime,TotalFee");

            lines.AddRange(data.Select(formatter));
            File.WriteAllLines(fileName, lines);
        }

        // --- Parsers ---
        private static User ParseUser(string[] v) => new User { Username = v[0], Password = v[1], FullName = v[2], Role = (UserRole)Enum.Parse(typeof(UserRole), v[3]) };
        private static ParkingSlot ParseSlot(string[] v) => new ParkingSlot { SlotNumber = v[0], Type = (VehicleType)Enum.Parse(typeof(VehicleType), v[1]) };
        private static ParkingRecord ParseRecord(string[] v) => new ParkingRecord
        {
            TicketNumber = v[0],
            VehicleNumber = v[1],
            OwnerName = v[2],
            Type = (VehicleType)Enum.Parse(typeof(VehicleType), v[3]),
            AllocatedSlotNumber = v[4],
            EntryTime = DateTime.Parse(v[5]),
            ExitTime = string.IsNullOrWhiteSpace(v[6]) ? (DateTime?)null : DateTime.Parse(v[6]),
            TotalFee = string.IsNullOrWhiteSpace(v[7]) ? (decimal?)null : decimal.Parse(v[7])
        };

        // --- Default Data Providers ---
        private static List<User> GetDefaultUsers() => new List<User>
        {
            new User { Username = "admin", Password = "pass123", FullName = "Manish Suthar", Role = UserRole.Admin },
            new User { Username = "attendant1", Password = "pass123", FullName = "Sandip Limbasiya", Role = UserRole.Attendant }
        };
        private static List<ParkingSlot> GetDefaultSlots()
        {
            var defaultSlots = new List<ParkingSlot>();
            for (int i = 1; i <= 20; i++) defaultSlots.Add(new ParkingSlot { SlotNumber = $"A{i:D2}", Type = VehicleType.FourWheeler });
            for (int i = 1; i <= 10; i++) defaultSlots.Add(new ParkingSlot { SlotNumber = $"B{i:D2}", Type = VehicleType.TwoWheeler });
            return defaultSlots;
        }
    }
    #endregion

    #region Manager Modules
    public static class AdminManager
    {
        // Methods now receive all data lists they need, making them more self-contained.
        public static void ShowAdminMenu(List<User> users, List<ParkingSlot> slots, List<ParkingRecord> ledger)
        {
            bool exit = false;
            while (!exit)
            {
                var menu = new Dictionary<string, string>
                {
                    { "1", "Manage Parking Slots" },
                    { "2", "Manage Users" },
                    { "9", "Back to Main Menu" }
                };
                Console.Clear();
                UIHelper.DrawMenu("Admin Functions", menu);
                Console.Write("Enter your choice: ");
                switch (Console.ReadLine())
                {
                    case "1": ManageSlots(users, slots, ledger); break;
                    case "2": ManageUsers(users); break;
                    case "9": exit = true; break;
                    default: UIHelper.PrintError("Invalid choice."); UIHelper.Pause(); break;
                }
            }
        }

        private static void ManageSlots(List<User> users, List<ParkingSlot> slots, List<ParkingRecord> ledger)
        {
            Console.Clear();
            UIHelper.DrawHeader("Manage Parking Slots");
            Console.WriteLine("Current Slots:");
            foreach (var slot in slots.OrderBy(s => s.SlotNumber)) Console.WriteLine($"- {slot.SlotNumber} ({slot.Type})");

            Console.WriteLine("\nOptions: [a]dd / [r]emove / [b]ack");
            Console.Write("Choose an option: ");
            switch (Console.ReadLine().ToLower())
            {
                case "a":
                    Console.Write("Enter new slot number (e.g., C01): ");
                    string newSlotNum = Console.ReadLine().ToUpper();
                    if (slots.Any(s => s.SlotNumber.Equals(newSlotNum, StringComparison.OrdinalIgnoreCase)))
                    {
                        UIHelper.PrintError("A slot with this number already exists.");
                        break;
                    }
                    Console.Write("Enter type (1 for 4-Wheeler, 2 for 2-Wheeler): ");
                    var type = Console.ReadLine() == "1" ? VehicleType.FourWheeler : VehicleType.TwoWheeler;
                    slots.Add(new ParkingSlot { SlotNumber = newSlotNum, Type = type });
                    UIHelper.PrintSuccess("Slot added.");
                    break;
                case "r":
                    Console.Write("Enter slot number to remove: ");
                    string slotToRemove = Console.ReadLine().ToUpper();
                    int removedCount = slots.RemoveAll(s => s.SlotNumber.Equals(slotToRemove, StringComparison.OrdinalIgnoreCase));
                    if (removedCount > 0) UIHelper.PrintSuccess("Slot removed.");
                    else UIHelper.PrintError("Slot not found.");
                    break;
                case "b":
                    return;
            }
            // Save all data after a change
            DataManager.SaveAllData(users, slots, ledger);
            UIHelper.Pause();
        }

        private static void ManageUsers(List<User> users)
        {
            // Simplified user management for brevity
            Console.Clear();
            UIHelper.DrawHeader("Manage Users");
            Console.WriteLine("Current Users:");
            foreach (var user in users) Console.WriteLine($"- {user.Username} ({user.Role})");
            UIHelper.PrintError("\nUser management functionality is simplified for this version.");
            UIHelper.Pause();
        }
    }

    public static class ReportManager
    {
        public static void ShowReportMenu(List<ParkingRecord> ledger)
        {
            bool exit = false;
            while (!exit)
            {
                var menu = new Dictionary<string, string>
                {
                    { "1", "View Currently Parked Vehicles" },
                    { "2", "View Daily Income Report" },
                    { "9", "Back to Main Menu" }
                };
                Console.Clear();
                UIHelper.DrawMenu("System Reports", menu);
                Console.Write("Enter your choice: ");
                switch (Console.ReadLine())
                {
                    case "1": ViewCurrentlyParked(ledger); break;
                    case "2": ViewDailyIncome(ledger); break;
                    case "9": exit = true; break;
                    default: UIHelper.PrintError("Invalid choice."); UIHelper.Pause(); break;
                }
            }
        }

        private static void ViewCurrentlyParked(List<ParkingRecord> ledger)
        {
            Console.Clear();
            UIHelper.DrawHeader("Currently Parked Vehicles");
            var currentlyParked = ledger.Where(p => p.ExitTime == null).OrderBy(p => p.EntryTime).ToList();
            if (currentlyParked.Any())
            {
                Console.WriteLine($"{"Slot",-10}{"Vehicle No.",-15}{"Owner",-20}{"Entry Time",-20}");
                Console.WriteLine(new string('-', 65));
                foreach (var v in currentlyParked)
                {
                    Console.WriteLine($"{v.AllocatedSlotNumber,-10}{v.VehicleNumber,-15}{v.OwnerName,-20}{v.EntryTime:g,-20}");
                }
            }
            else
            {
                Console.WriteLine("The parking lot is currently empty.");
            }
            UIHelper.Pause();
        }

        private static void ViewDailyIncome(List<ParkingRecord> ledger)
        {
            Console.Clear();
            UIHelper.DrawHeader("Daily Income Report");
            var dailyRecords = ledger
                .Where(p => p.ExitTime.HasValue && p.ExitTime.Value.Date == DateTime.Today)
                .ToList();

            if (dailyRecords.Any())
            {
                decimal totalIncome = dailyRecords.Sum(p => p.TotalFee ?? 0);
                Console.WriteLine($"Total Vehicles Exited Today: {dailyRecords.Count}");
                UIHelper.PrintSuccess($"Total Income for {DateTime.Today:yyyy-MM-dd}: ${totalIncome:F2}");
                Console.WriteLine("\n--- Individual Records ---");
                foreach (var v in dailyRecords.OrderBy(rec => rec.ExitTime))
                {
                    Console.WriteLine($"- {v.VehicleNumber} ({v.ExitTime:T}): ${v.TotalFee:F2}");
                }
            }
            else
            {
                Console.WriteLine($"No vehicles have exited and paid today ({DateTime.Today:yyyy-MM-dd}).");
            }
            UIHelper.Pause();
        }
    }
    #endregion

    #region UI Helper
    public static class UIHelper
    {
        public static void DrawHeader(string title)
        {
            Console.WriteLine("=======================================================");
            Console.WriteLine($"  {title}");
            Console.WriteLine("=======================================================");
        }

        public static void DrawMenu(string title, Dictionary<string, string> menuItems)
        {
            Console.WriteLine($"\n--- {title} ---");
            foreach (var item in menuItems)
            {
                Console.WriteLine($"{item.Key}. {item.Value}");
            }
            Console.WriteLine("---------------------------------");
        }

        public static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void PrintInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void Pause()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        public static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, (password.Length - 1));
                    Console.Write("\b \b");
                }
            } while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return password;
        }

        public static void ShowAccessDenied()
        {
            PrintError("\nACCESS DENIED: Your role does not have permission for this feature.");
            Pause();
        }
    }
    #endregion
}
