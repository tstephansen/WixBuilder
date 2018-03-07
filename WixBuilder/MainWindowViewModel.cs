using LandmarkDevs.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WixBuilder
{
    public class MainWindowViewModel : Observable
    {
        public MainWindowViewModel()
        {
            FileDictionary = new Dictionary<string, Dictionary<string, string>>();
            DirectoryDictionary = new Dictionary<string, DirectoryInfo>();
            Directories = new ObservableDictionary<string, string>();
            AddDirectoryCommand = new RelayCommand(AddDirectory);
            RemoveDirectoryCommand = new RelayCommand<string>(RemoveDirectory);
            ProcessCommand = new RelayCommand(async () => await ProcessDependencies());
        }

        #region Commands
        public RelayCommand AddDirectoryCommand { get; }
        public RelayCommand<string> RemoveDirectoryCommand { get; }
        public RelayCommand ProcessCommand { get; }
        #endregion

        #region Methods
        public void AddDirectory()
        {
            if (string.IsNullOrEmpty(DirectoryName) || string.IsNullOrEmpty(VariableName)) return;
            Directories.Add(DirectoryName, VariableName);
            DirectoryName = string.Empty;
            VariableName = string.Empty;
        }

        public void RemoveDirectory(string directoryName) => Directories.Remove(directoryName);

        public Task ProcessDependencies()
        {
            return Task.Run(() =>
            {
                GetDirectoryInformation();
                foreach (var info in DirectoryDictionary.Values)
                {
                    GetListOfFilesInDirectory(info);
                }
                WriteDependencyInformationToFile();
                Application.Current.Dispatcher.Invoke(() => { MessageBox.Show("Complete!"); });
            });
        }

        public void GetListOfFilesInDirectory(DirectoryInfo directoryInfo)
        {
            GetFilesInDirectory(directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly));
        }

        public void GetFilesInDirectory(FileInfo[] files)
        {
            foreach (var file in files)
            {
                if (FileDictionary.ContainsKey(file.Name))
                    continue;
                FileDictionary.Add(file.Name, new Dictionary<string, string> { { file.Directory.FullName, file.Name } });
            }
        }

        public void GetDirectoryInformation()
        {
            foreach (var directoryPath in Directories.Keys)
            {
                var dir = new DirectoryInfo(directoryPath);
                DirectoryDictionary.Add(dir.FullName, dir);
            }
        }

        public void WriteDependencyInformationToFile()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<Fragment>");
            sb.AppendLine("<DirectoryRef Id=\"InstallDirectory\">");
            var counter = 0;
            foreach (var file in FileDictionary)
            {
                var value = file.Value;
                var variableName = Directories.First(c => c.Key == value.Keys.ElementAt(0) || c.Key.Contains(value.Keys.ElementAt(0))).Value;
                sb.AppendLine($"<Component Id=\"cmp{counter}\" Guid=\"{Guid.NewGuid()}\">");
                sb.AppendLine(
                    $"<File Id=\"file{counter}\" KeyPath=\"yes\" Source=\"{variableName}{value.Values.ElementAt(0)}\" />");
                sb.AppendLine("</Component>");
                counter++;
            }
            sb.AppendLine("</DirectoryRef>");
            sb.AppendLine("</Fragment>");
            if(File.Exists(DependencyFilePath)) File.Delete(DependencyFilePath);
            using (var fs = new FileStream(DependencyFilePath, FileMode.Create, FileAccess.ReadWrite))
            {
                var bytes = new UTF8Encoding(true).GetBytes(sb.ToString());
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();
            }
        }
        #endregion

        #region Fields and Properties
        public Dictionary<string, Dictionary<string, string>> FileDictionary
        {
            get => _fileDictionary;
            set => Set(ref _fileDictionary, value);
        }
        private Dictionary<string, Dictionary<string, string>> _fileDictionary;

        public Dictionary<string, DirectoryInfo> DirectoryDictionary
        {
            get => _directoryDictionary;
            set => Set(ref _directoryDictionary, value);
        }
        private Dictionary<string, DirectoryInfo> _directoryDictionary;
        
        public string DependencyFilePath
        {
            get => _dependencyFilePath;
            set => Set(ref _dependencyFilePath, value);
        }
        private string _dependencyFilePath;

        public ObservableDictionary<string, string> Directories
        {
            get => _directories;
            set => Set(ref _directories, value);
        }
        private ObservableDictionary<string, string> _directories;

        public string DirectoryName
        {
            get => _directoryName;
            set => Set(ref _directoryName, value);
        }
        private string _directoryName;

        public string VariableName
        {
            get => _variableName;
            set => Set(ref _variableName, value);
        }
        private string _variableName;
        #endregion
    }
}

