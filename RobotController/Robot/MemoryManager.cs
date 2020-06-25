using RobotController.util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RobotController.Robot
{
    public class MemoryManager
    {
        public int playedMemoryID = 0;
        public int savedMemoriesCount;
        public string memoryFolder = "./SavedMemories/";
        public string memoryFilePrefix = "mem";
        public string memoryFilePostfix = ".dat";
        public Memory working;

        public ConnectedRobot robot;
        public List<string> availableMemories;

        public MemoryRecordingState state = MemoryRecordingState.ready;
        
        public TimedBackgroundThread playThread;
        public int playTick = 0;

        public MemoryManager(ConnectedRobot robot)
        {
            this.robot = robot;
            playThread = new TimedBackgroundThread(40, playThreadMain);
            playThread.Start();
            loadMemories();
        }

        private void playThreadMain()
        {
            if(state == MemoryRecordingState.playback)
            {
                if(working != null)
                {
                    if(playTick < working.data.Count)
                    {
                        robot.updateDisplay(working.data[playTick]);
                        playTick++;
                    }
                    else
                    {
                        playTick = 0;
                    }
                }
            }
        }

        public void loadMemories()
        {
            Directory.CreateDirectory(memoryFolder);
            savedMemoriesCount = robot.settings.readInt("currentSavedMemories", 0);

            availableMemories = new List<string>(savedMemoriesCount);

            for(int i = 0; i < savedMemoriesCount; i++)
            {
                string fileName = getFilenameFromID(i);

                if(File.Exists(fileName))
                {
                    availableMemories.Add(fileName);
                }

            }

            string[] files = Directory.GetFiles(memoryFolder);

            for (int i = 0; i < files.Length; i++)
            {
                if (File.Exists(files[i]) && !availableMemories.Contains(files[i]) && files[i].EndsWith(memoryFilePostfix))
                {
                    availableMemories.Add(files[i]);
                }

            }

            savedMemoriesCount = availableMemories.Count;

            robot.updateMemoryState("Loaded: " + savedMemoriesCount);
        }

        public void addToWorking(datapoint d)
        {
            if(state == MemoryRecordingState.recording)
            {
                if(working != null)
                {
                    working.data.Add(d);
                }
            }
        }

        public void Play()
        {
            if(state == MemoryRecordingState.ready)
            {
                for(;playedMemoryID < availableMemories.Count; playedMemoryID++)
                {
                    working = Memory.loadFromDisk(availableMemories[playedMemoryID]);
                    if (working != null)
                    {
                        playTick = 0;
                        state = MemoryRecordingState.playback;
                        robot.updateMemoryState("Playback: " + savedMemoriesCount);
                        break;
                    }
                }

            }
        }

        public void Record()
        {
            if(state == MemoryRecordingState.ready)
            {
                savedMemoriesCount++;
                robot.settings.saveString("currentSavedMemories", savedMemoriesCount + "");
                working = new Memory(getFilenameFromID(savedMemoriesCount));
                working.saveToDisk();
                state = MemoryRecordingState.recording;
                robot.updateMemoryState("recording: " + savedMemoriesCount);
            }
        }

        public void Stop()
        {
            if(state == MemoryRecordingState.recording)
            {
                state = MemoryRecordingState.stopping;
                working.saveToDisk();
                robot.updateMemoryState("stopping: " + savedMemoriesCount + " after " + working.totalTimeSeconds);
                working = null;
                state = MemoryRecordingState.ready;
            }

            if(state == MemoryRecordingState.playback)
            {
                working = null;
                state = MemoryRecordingState.ready;
            }
        }

        private string getFilenameFromID(int id)
        {
            return memoryFolder + memoryFilePrefix + id + memoryFilePostfix;
        }

        public enum MemoryRecordingState
        {
            ready = 0,
            recording = 1,
            stopping = 2,
            playback = 3,
        }
    }

    [Serializable]
    public class Memory
    {
        public string filename;
        public long startTime;
        public long totalTimeSeconds;
        public List<datapoint> data;

        public Memory(string filename)
        {
            this.filename = filename;
            startTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            totalTimeSeconds = 0;
            data = new List<datapoint>();
        }

        public void saveToDisk()
        {
            totalTimeSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - startTime;
            using (Stream stream = File.Open(filename, FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, this);
                stream.Close();
            }
        }

        public static Memory loadFromDisk(string filename)
        {
            using (Stream stream = File.Open(filename, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                if(stream.Length > 0)
                {
                    return (Memory)binaryFormatter.Deserialize(stream);
                }

                return null;
            }
        }
    }
}
