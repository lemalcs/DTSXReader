using System.Security;

namespace DTSXExplorer
{
    /// <summary>
    /// Holds passwords.
    /// </summary>
    public interface IPasswordContainer
    {
        SecureString password { get; }

        void Clear();
    }
}
