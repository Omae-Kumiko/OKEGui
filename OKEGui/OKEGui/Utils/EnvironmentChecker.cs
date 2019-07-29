﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OKEGui.Utils
{
    static class EnvironmentChecker
    {
        static OKEGuiConfig Config;
        public static Boolean CheckEnviornment()
        {
            if (!CheckRootFolderWriteAccess())
            {
                MessageBox.Show("没有权限控制OKEGui所在的文件夹，请保证当前用户获取了目录权限，或者以管理员模式运行。", "没有权限控制OKEGui所在的文件夹");
                return false;
            }
            Config = ConfigManager.LoadConfig();

            if (!CheckVspipe())
            {
                return false;
            }

            return true;
        }

        static Boolean CheckRootFolderWriteAccess()
        {
            FileIOPermission f = new FileIOPermission(FileIOPermissionAccess.AllAccess, AppDomain.CurrentDomain.BaseDirectory);
            try
            {
                f.Demand();
            }
            catch (SecurityException)
            {
                return false;
            }
            return true;
        }

        static Boolean CheckVspipe()
        {
            string vspipePath = Config.vspipePath;
            FileInfo vspipeInfo;
            if (string.IsNullOrWhiteSpace(vspipePath))
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey("software\\vapoursynth");
                if (key == null)
                {
                    key = Registry.CurrentUser.OpenSubKey("software\\vapoursynth");
                }
                if (key != null)
                {
                    vspipePath = key.GetValue("Path") as string;
                }
                if (!string.IsNullOrWhiteSpace(vspipePath))
                {
                    vspipePath += "\\core64\\vspipe.exe";
                }
            }

            if (!string.IsNullOrWhiteSpace(vspipePath))
            {
                vspipeInfo = new FileInfo(vspipePath);

                if (vspipeInfo.Exists)
                {
                    Config.vspipePath = vspipePath;
                    return true;
                }
            }

            MessageBox.Show(
                        "无法找到vspipe.exe。请手动指定其位置，否则程序将退出。",
                        "无法找到vspipe.exe",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

            OpenFileDialog ofd = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "vspipe.exe (vspipe.exe)|vspipe.exe"
            };
            bool result = ofd.ShowDialog().GetValueOrDefault(false);
            if (!result)
            {
                return false;
            }
            vspipePath = ofd.FileName;
            vspipeInfo = new FileInfo(vspipePath);

            if (vspipeInfo.Exists)
            {
                Config.vspipePath = vspipePath;
                return true;
            }
            else
            {
                MessageBox.Show("请尝试重新安装VapourSynth，程序将退出。", "此文件无法读取");
                return false;
            }
        }
    }
}
