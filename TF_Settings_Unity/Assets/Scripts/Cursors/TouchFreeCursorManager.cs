using System.IO;
using Ultraleap.TouchFree.ServiceShared;
using UnityEngine;

namespace Ultraleap.TouchFree
{
    public class TouchFreeCursorManager : CursorManager
    {
        protected Color Primary;
        protected Color Secondary;
        protected Color Tertiary;

        private FileSystemWatcher watcher;
        private FileSystemEventHandler watcherHandler;
        private bool fileChanged = true;

        protected override void OnEnable()
        {
            watcher.Changed += watcherHandler;
            fileChanged = true;
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            watcher.Changed -= watcherHandler;
            base.OnDisable();
        }

        void Awake()
        {
            Primary = defaultCursor.primaryColor;
            Secondary = defaultCursor.secondaryColor;
            Tertiary = defaultCursor.tertiaryColor;

            watcher = new FileSystemWatcher();
            watcher.Path = ConfigFileUtils.ConfigFileDirectory;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.NotifyFilter = NotifyFilters.LastAccess;
            watcher.Filter = TouchFreeAppConfigFile.ConfigFileName;
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            watcherHandler = new FileSystemEventHandler(FileUpdated);
        }

        private void FileUpdated(object source, FileSystemEventArgs e)
        {
            fileChanged = true;
        }

        private void Update()
        {
            if (fileChanged)
            {
                TFAppConfig.Refresh();

                SetCursorVisibility(TFAppConfig.Config.cursorEnabled);

                var cursorSize = TFAppConfig.Config.cursorSizeCm;

                TFAppConfig.Config.GetCurrentColors(ref Primary, ref Secondary, ref Tertiary);
                foreach (InteractionCursor cursor in interactionCursors)
                {
                    cursor.cursor.SetRingThickness(TFAppConfig.Config.cursorRingThickness);
                    cursor.cursor.cursorSize = cursorSize;
                    cursor.cursor.SetColors(Primary, Secondary, Tertiary);
                }

                fileChanged = false;
            }
        }
    }
}