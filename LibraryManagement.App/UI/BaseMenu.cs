using LibraryManagement.Core.Entities;

namespace LibraryManagement.App.UI
{
    internal abstract class BaseMenu
    {
        protected readonly SessionContext _session;

        protected BaseMenu(SessionContext session)
        {
            _session = session;
        }

        public void Run()
        {
            while (true)
            {
                var currentUser = _session.CurrentUser!;
                ConsoleIO.WaitForKey();
                DisplayMenu(currentUser);
                var choice = ConsoleIO.ReadMenuChoice("Choose an option: ", 0, MaxOption);

                if (choice == 0)
                {
                    ConsoleIO.Clear();
                    _session.Clear();
                    return;
                }

                HandleChoice(choice, currentUser);
            }
        }

        protected abstract int MaxOption { get; }
        protected abstract void DisplayMenu(UserEntity currentUser);
        protected abstract void HandleChoice(int choice, UserEntity currentUser);
    }
}