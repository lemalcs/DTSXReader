using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Input;

namespace DTSXExplorer
{
    /// <summary>
    /// Types of authentication for SQL Server.
    /// </summary>
    public enum AuthenticationType
    {
        /// <summary>
        /// Windows authentication.
        /// </summary>
        Windows = 0,

        /// <summary>
        /// Authentication used by SQL Server.
        /// </summary>
        SQLServer = 1
    }

    public class SQLServerAuthenticationViewModel : BaseModel
    {
        #region Properties

        /// <summary>
        /// SQL Server instance name.
        /// </summary>
        private string serverName;

        /// <summary>
        /// The database name
        /// </summary>
        private string databaseName;

        /// <summary>
        /// User name used to connect to SQL Server.
        /// </summary>
        private string userName;

        /// <summary>
        /// The password to login to SQL Server.
        /// </summary>
        private SecureString password;

        /// <summary>
        /// The type of authentication.
        /// </summary>
        private AuthenticationType authentication;

        /// <summary>
        /// The message that shows the result of connection test.
        /// </summary>
        private string connectionResult;

        /// <summary>
        /// Indicates whether there is a connection attempt to a database.
        /// </summary>
        private bool isConnecting;

        /// <summary>
        /// Performs the connection to SQL Server instance in background.
        /// </summary>
        private BackgroundWorker worker;

        public string ServerName
        {
            get => serverName;
            set
            {
                if (serverName != value)
                {
                    serverName = value;
                    OnPropertyChanged(nameof(ServerName));
                }
            }
        }

        public string UserName
        {
            get => userName;
            set
            {
                if (userName != value)
                {
                    userName = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }

        public SecureString Password { get => password; private set => password = value; }

        public AuthenticationType Authentication
        {
            get => authentication;
            set
            {
                if (authentication != value)
                {
                    authentication = value;
                    OnPropertyChanged(nameof(Authentication));

                    if (Authentication == AuthenticationType.Windows)
                        UserName = Environment.UserName;
                    else
                        UserName = string.Empty;
                }
            }
        }
        public string DatabaseName
        {
            get => databaseName;
            set
            {
                if (databaseName != value)
                {
                    databaseName = value;
                    OnPropertyChanged(nameof(DatabaseName));
                }
            }
        }
        public string ConnectionResult
        {
            get => connectionResult;
            set
            {
                if (connectionResult != value)
                {
                    connectionResult = value;
                    OnPropertyChanged(nameof(ConnectionResult));
                }
            }
        }
        public bool IsConnecting
        {
            get => isConnecting;
            set
            {
                if (isConnecting != value)
                {
                    isConnecting = value;
                    OnPropertyChanged(nameof(IsConnecting));
                }
            }
        }

        #endregion

        #region Commands

        public ICommand TestConnectionCommand { get; }
        public ICommand SetPasswordCommand { get; }

        #endregion

        public SQLServerAuthenticationViewModel()
        {
            TestConnectionCommand = new RelayCommand(TestConnection);
            SetPasswordCommand = new RelayCommand(SetPassword);

            // Set windows authentication as default
            Authentication = AuthenticationType.Windows;
            UserName = Environment.UserName;
        }

        private void SetPassword(object parameter)
        {
            if (Authentication == AuthenticationType.SQLServer)
                Password = ((IPasswordContainer)parameter).password;
        }

        /// <summary>
        /// Test connection to a SQL Server instance.
        /// </summary>
        private void TestConnection(object parameter)
        {
            worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.RunWorkerAsync(parameter);

            IsConnecting = true;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsConnecting = false;

            if (e.Error != null)
                ConnectionResult = $"Error: {e.Error.Message}";
            else
                ConnectionResult = "Connection succeeded!";

            worker.DoWork -= Worker_DoWork;
            worker.RunWorkerCompleted -= Worker_RunWorkerCompleted;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {

            SqlConnection connection = new SqlConnection();
            SqlConnectionStringBuilder connectionString = new SqlConnectionStringBuilder();
            try
            {
                connectionString.DataSource = ServerName;
                connectionString.InitialCatalog = DatabaseName;

                if (Authentication == AuthenticationType.Windows)
                {
                    connectionString.UserID = UserName;
                    connectionString.IntegratedSecurity = true;
                }
                else
                {
                    connectionString.UserID = UserName;
                    connectionString.Password = RetrieveString(((IPasswordContainer)e.Argument).password);
                }

                connection.ConnectionString = connectionString.ToString();
                connectionString.Clear();
                connection.Open();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                connection.Close();
            }
        }

        /// <summary>
        /// Get the actual password from <see cref="SecureString"/> container.
        /// </summary>
        /// <param name="secString">The <see cref="SecureString"/> that contains the password.</param>
        /// <returns>A <see cref="string"/> value with the password.</returns>
        private string RetrieveString(SecureString secString)
        {
            if (secString == null)
                return null;

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secString);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        public string GetConnectionString()
        {
            SqlConnectionStringBuilder connectionString = new SqlConnectionStringBuilder();

            connectionString.DataSource = ServerName;
            connectionString.InitialCatalog = DatabaseName;

            if (Authentication == AuthenticationType.Windows)
            {
                connectionString.UserID = UserName;
                connectionString.IntegratedSecurity = true;
            }
            else
            {
                connectionString.UserID = UserName;
                connectionString.Password = RetrieveString(Password);
            }

            return connectionString.ToString();
        }
    }
}
