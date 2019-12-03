using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

#if !Editor
using UnityEngine;
#endif

namespace GEngine
{

    class LogFile
    {
        public virtual void WriteLine(string msg) { }
        public virtual void Close() { }
    }

    class LogFileLocal : LogFile
    {
        private TextWriter _writer;
        private FileStream _fileStream;

        public LogFileLocal()
        {
            CleanFile();

            System.DateTime now = DateTime.Now;
            string fileName = $"engine-{now.Year}{now.Month:00}{now.Day:00}-{now.Hour:00}{now.Minute:00}{now.Second:00}.log";
            _fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read);
            _writer = new StreamWriter(_fileStream, Encoding.UTF8);
        }

        private void CleanFile()
        {
            List<FileInfo> fileList = new List<FileInfo>();
            DirectoryInfo dir = new DirectoryInfo("./");
            FileSystemInfo[] files = dir.GetFileSystemInfos();
            foreach (FileSystemInfo t in files)
            {
                // 不处理 DirectoryInfo 目录，只处理文件
                FileInfo file = t as FileInfo;
                if (file != null)
                {
                    if (file.FullName.Substring(file.FullName.LastIndexOf(".", StringComparison.Ordinal))
                             .ToLower() == ".log" && file.FullName.Contains("engine-"))
                    {
                        fileList.Add(file);
                    }
                }
            }

            fileList.Sort((x, y) => String.Compare(x.FullName, y.FullName, StringComparison.Ordinal));

            // log文件大于5个，就清除之前的
            while (fileList.Count > 5)
            {
                File.Delete(fileList[0].FullName);
                fileList.RemoveAt(0);
            }
        }

        public override void WriteLine(string msg)
        {
            string s = msg + "\r\n";
            _writer.Write(s);
            _writer.Flush();
        }

        public override void Close()
        {
            _writer.Close();
            _fileStream.Close();
        }
    }

    class GameLogger : SingletonObject<GameLogger>, System.IDisposable
    {
        private LogFile _logFile;

        public GameLogger()
        {
            Start();
        }

        public void Dispose()
        {
            if (_logFile != null)
                _logFile.Close();
        }

        public void Start()
        {
            if (_logFile != null)
                return;

            if (Application.platform != RuntimePlatform.WindowsEditor &&
                 Application.platform != RuntimePlatform.WindowsPlayer)
            {
                _logFile = new LogFile();
            }
            else
            {
                try
                {
                    _logFile = new LogFileLocal();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                    _logFile = new LogFile();
                }
            }
        }

#region 仅在内部调试环境下输出

        [Conditional("TRACE")]
        public void Trace(string s)
        {
            Output(s);
        }

        [Conditional("TRACE")]
        public void Trace(string fmt, object arg)
        {
            Output(string.Format(fmt, arg));
        }

        [Conditional("TRACE")]
        public void Trace(string fmt, params object[] args)
        {
            Output(string.Format(fmt, args));
        }

#endregion

#region 外部接口

        public void Output(string msg)
        {
#if DEBUG
            UnityEngine.Debug.Log(msg);
#endif
            _logFile.WriteLine(msg);
        }

        [Conditional("DEBUG")]
        public void Debug(string fmt, params object[] args)
        {
            Output(string.Format(fmt, args));
        }

        [Conditional("DEBUG")]
        public void Debug(string fmt, object arg)
        {
            Output(string.Format(fmt, arg));
        }

        [Conditional("DEBUG")]
        public void Debug(object arg)
        {
            Output(arg == null ? "null" : arg.ToString());
        }

#endregion

    }
}