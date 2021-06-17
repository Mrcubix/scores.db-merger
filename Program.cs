using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace scores.db_merger
{
    class Program
    {
        static void Main(string[] args)
        {
            ScoresDB file1 = new ScoresDB("./input/scores1.db");
            Console.WriteLine(file1.maps[0].BeatmapMD5hash.decodedstring);
            ScoresDB file2 = new ScoresDB("./input/scores.db");
            Console.WriteLine(file2.maps[0].BeatmapMD5hash.decodedstring);
            ScoresDB file3 = Merge(file1, file2);
            Console.WriteLine(file2.maps[0].BeatmapMD5hash.decodedstring);
            file3.writeToScoresDB();
        }
        static ScoresDB Merge(ScoresDB file1, ScoresDB file2) 
        {
            ScoresDB output = new ScoresDB(file1);
            for (int mapIndexA = 0; mapIndexA != file2.maps.Count; mapIndexA++)
            {
                for (int mapIndexB = 0; mapIndexB != file1.maps.Count; mapIndexB++)
                {
                    if (file2.maps[mapIndexA].BeatmapMD5hash.decodedstring == file1.maps[mapIndexB].BeatmapMD5hash.decodedstring)
                    {
                        for (int scoreIndexA = 0; scoreIndexA != file2.maps[mapIndexA].scores.Count; scoreIndexA++) 
                        {
                            if (!file2.maps[mapIndexB].scores[scoreIndexA].isContainedIn(file1.maps[mapIndexB].scores)) 
                            {
                                output.maps[mapIndexB].scores.Add(file2.maps[mapIndexB].scores[scoreIndexA]);
                                output.maps[mapIndexB].scoreCount++;
                                Console.WriteLine($"Added: {file2.maps[mapIndexB].scores[scoreIndexA].RPHash.decodedstring}, Type: Score");
                            }
                        }
                    }
                }
            }
            List<string> hashes = file1.gethashList();
            for (int i = 0; i != file2.maps.Count; i++)
            {
                if (!hashes.Contains(file2.maps[i].BeatmapMD5hash.decodedstring)) 
                {
                    output.maps.Add(file2.maps[i]);
                    output.mapCount++;
                    Console.WriteLine($"Added: {file2.maps[i].BeatmapMD5hash.decodedstring}, Type: Map");
                }
            }
            return output;
        }
    }
    class ScoresDB
    {
        public int version;
        public int mapCount;
        public List<Beatmap> maps = new List<Beatmap>();

        public ScoresDB(string filename) 
        {
            // Read all bytes and store them in file
            byte[] bytes = File.ReadAllBytes(filename);
            // pass the bytes to the memory stream (faster than reading from disk directly)
            using (MemoryStream stream = new MemoryStream(bytes))
            using (var reader = new BinaryReader(stream)) 
            {
                // Read scores.db version
                version = reader.ReadInt32();
                // Read the number of beatmaps with scores
                mapCount = reader.ReadInt32();
                // Show the number of maps in the console
                Console.WriteLine($"number of maps : {mapCount}");
                // For each beatmaps with scores
                for (int i=0; i != mapCount; i++) 
                {
                    Beatmap map = new Beatmap(reader);
                    maps.Add(map);
                }
            }
        }
        public ScoresDB(ScoresDB instance)
        {
            version = instance.version;
            mapCount = instance.mapCount;
            Console.WriteLine($"number of maps : {mapCount}");
            maps = new List<Beatmap>(instance.maps);
        }
        public List<string> gethashList() 
        {
            List<string> hashes = new List<string>();
            for (int i = 0; i != this.maps.Count; i++) 
            {
                hashes.Add(this.maps[i].BeatmapMD5hash.decodedstring);
            }
            return hashes;
        }
        public void writeToScoresDB() 
        {
            Directory.CreateDirectory("output");
            using (var stream = File.Create("./output/scores.db"))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(version);
                writer.Write(mapCount);
                for (int i=0; i != mapCount; i++) 
                {
                    maps[i].writeToScoresDB(writer);
                }
            }
        }
    }
    class Beatmap
    {
        public DecodedString BeatmapMD5hash;
        public int scoreCount;
        public List<Score> scores = new List<Score>();
        public Beatmap(BinaryReader reader)
        {
            // Read the MD5 hash of the map
            BeatmapMD5hash = new DecodedString(reader);
            // Read the number of scores on the map
            scoreCount = reader.ReadInt32();
            // for each score in map
            for (int j=0; j != scoreCount; j++) 
            {
                Score score = new Score(reader);
                scores.Add(score);
            }
        }
        public void writeToScoresDB(BinaryWriter writer)
        {
            BeatmapMD5hash.writeToScoresDB(writer);
            writer.Write(scoreCount);
            for (int j=0; j != scoreCount; j++) 
            {
                scores[j].writeToScoresDB(writer);
            }
        }
    }
    class Score
    {
        public byte gamemode;
        public int version;
        public DecodedString BMhash;
        public DecodedString username;
        public DecodedString RPHash;
        public ushort h300;
        public ushort h100;
        public ushort h50;
        public ushort hgekis;
        public ushort hkatus;
        public ushort hmiss;
        public int RPscore;
        public ushort maxCombo;
        public bool PFCombo;
        public int mods;
        public DecodedString empty;
        public long timestamp;
        public int minusone;
        public long scoreID;

        public Score(BinaryReader reader)
        {
            gamemode = reader.ReadByte();
            version = reader.ReadInt32();
            BMhash = new DecodedString(reader);
            username = new DecodedString(reader);
            RPHash = new DecodedString(reader);
            h300 = reader.ReadUInt16();
            h100 = reader.ReadUInt16();
            h50 = reader.ReadUInt16();
            hgekis = reader.ReadUInt16();
            hkatus = reader.ReadUInt16();
            hmiss = reader.ReadUInt16();
            RPscore = reader.ReadInt32();
            maxCombo = reader.ReadUInt16();
            PFCombo = reader.ReadBoolean();
            mods = reader.ReadInt32();
            empty = new DecodedString(reader);
            timestamp = reader.ReadInt64();
            minusone = reader.ReadInt32();
            scoreID = reader.ReadInt64();
        }
        public bool isContainedIn(List<Score> scores) 
        {
            for (int i = 0; i != scores.Count; i++)
            {
                if (this.RPHash.decodedstring == scores[i].RPHash.decodedstring) 
                {
                    return true;
                }
            }
            return false;
        }
        public void writeToScoresDB(BinaryWriter writer)
        {
            writer.Write(gamemode);
            writer.Write(version);
            BMhash.writeToScoresDB(writer);
            username.writeToScoresDB(writer);
            RPHash.writeToScoresDB(writer);
            writer.Write(h300);
            writer.Write(h100);
            writer.Write(h50);
            writer.Write(hgekis);
            writer.Write(hkatus);
            writer.Write(hmiss);
            writer.Write(RPscore);
            writer.Write(maxCombo);
            writer.Write(PFCombo);
            writer.Write(mods);
            empty.writeToScoresDB(writer);
            writer.Write(timestamp);
            writer.Write(minusone);
            writer.Write(scoreID);
        }
    }

    class DecodedString
    {
        public int length;
        public string? decodedstring;
        public DecodedString(BinaryReader reader)
        {
            if (reader.ReadByte() == 0x0b)
            {
                // Get length of string
                length = reader.Read7BitEncodedInt();
                // Get length * bytes
                byte[] bytes = reader.ReadBytes(length);
                // convert byte array to string
                decodedstring = Encoding.UTF8.GetString(bytes);
            }
        }
        public void writeToScoresDB(BinaryWriter writer) 
        {
            if (decodedstring != null) 
            {
                writer.Write((Byte)0x0b);
                writer.Write7BitEncodedInt(length);
                writer.Write(Encoding.UTF8.GetBytes(decodedstring));
            }
            else
            {
                writer.Write((Byte)0x00);
            }
        }
    }
}