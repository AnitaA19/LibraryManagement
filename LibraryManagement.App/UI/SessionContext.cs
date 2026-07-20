using LibraryManagement.Core.Entities;

namespace LibraryManagement.App.UI
{
    internal class SessionContext
    {
        public UserEntity? CurrentUser { get; set; }

        public bool IsLoggedIn => CurrentUser != null;

        public void Clear()
        {
            CurrentUser = null;
        }
    }
}
