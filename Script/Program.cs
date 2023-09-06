//Copyright (c) Kirill Belozerov, 2023

//#define DEBUG 

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Script
{
    class Program
    {
#if DEBUG
        static void Main()
#else
        static void Main(string[] args)
#endif
        {
#if DEBUG
            string[] args = new string[2];
            args[0] = "CrystCoordinates.txt";
            args[1] = "CrystParameters.txt";
#endif

            string path1 = args[0];
            string path2 = args[1];

            ReadCoordinateFile(path1);
            SetCrystParameters(path2);

            for (int k = 0; k < NUMOFCOLS; k++)
            {
                Console.WriteLine("Start column");
                ColumnProcessing(path1);
                Console.WriteLine("Finish column");
            }
        }

        //current coordinates of the laser:
        public static int x = 0;
        public static int y = 0;
        public static int z = 0;

        public static int countCryst = 0;

        //Crystal Parameters:
        public static int NUMOFCOLS;
        public static int HDISTBETWCRYST;
        public static int WDISTBETWCRYST;
        public static int ALLOWEDH;
        public static int ALLOWEDW;
        public static int HBORDER1;
        public static int HBORDER2;
        public static int WBORDER1;
        public static int WBORDER2;
        public static int H1;
        public static int H2;
        public static int DX1;
        public static int DX2;
        public static int WIDTHOFCRYST;

        //structure (class) for keeping crystal's properties:
        public struct CrystCoord
        {
            public int x;
            public int y;
            public bool f1;
            public bool f2;
            public bool f3;
        }


        public static List<CrystCoord> crystCoords = new List<CrystCoord>();
        //
        enum Direction
        {
            TOP = 1,
            DOWN = 2
        };

        static void LaserOn()
        {
            Console.WriteLine("Laser on");
        }
        static void LaserOff()
        {
            Console.WriteLine("Laser off");
        }
        static void GoTo(int xNew, int yNew)
        {
            Console.WriteLine($"GoTo {xNew} {yNew}");
        }
        static void FromTopToDown(int x, ref int y, bool laserNecessary)
        {
            if (laserNecessary == true)
            {
                GoTo(x, y + H2);
                y += H2;
                LaserOn();
                GoTo(x, y + ALLOWEDH);
                y += ALLOWEDH;
                LaserOff();
                GoTo(x, y + H1);
                y += H1;
            }
            else
            {
                GoTo(x, y + H2 + ALLOWEDH + H1);
                y += H2 + ALLOWEDH + H1;
            }
        }
        static void FromDownToTop(int x, ref int y, bool laserNecessary)
        {
            if (laserNecessary == true)
            {
                GoTo(x, y - H1);
                y -= H1;
                LaserOn();
                GoTo(x, y - ALLOWEDH);
                y -= ALLOWEDH;
                LaserOff();
                GoTo(x, y - H2);
                y -= H2;
            }
            else
            {
                GoTo(x, y + H1 + ALLOWEDH + H2);
                y -= H1 + ALLOWEDH + H2;
            }
        }
        static void Autofocus()
        {
            Console.WriteLine("Autofocus 10 s +- 50 mkm z");
        }

        static void Delay(double timeInSec)
        {
            Console.WriteLine($"Delay {timeInSec} s");
        }

        static IEnumerable<string> ReadAllLinesFromFile(string path)
        {
            using (TextReader reader = File.OpenText(path))//creating a stream for reading file
            {
                string line;
                line = reader.ReadLine();
                while (line != null)
                {
                    yield return line;
                    line = reader.ReadLine();
                }
            }
        }

        static void SetCrystParameters(string path)
        {
            List<string> lines = new List<string>();

            foreach (string line in ReadAllLinesFromFile(path))
            {
                lines.Add(line);
            }

            string[] linesSplit = new string[lines.Count];
            for (int i = 0; i < 14; i++)
            {
                linesSplit[i] = lines[i].Split('\t')[1];
            }
            NUMOFCOLS = Int32.Parse(linesSplit[0]);
            HDISTBETWCRYST = Int32.Parse(linesSplit[1]);
            WDISTBETWCRYST = Int32.Parse(linesSplit[2]);
            ALLOWEDH = Int32.Parse(linesSplit[3]);
            ALLOWEDW = Int32.Parse(linesSplit[4]);
            HBORDER1 = Int32.Parse(linesSplit[5]);
            HBORDER2 = Int32.Parse(linesSplit[6]);
            WBORDER1 = Int32.Parse(linesSplit[7]);
            WBORDER2 = Int32.Parse(linesSplit[8]);
            H1 = Int32.Parse(linesSplit[9]);
            H2 = Int32.Parse(linesSplit[10]);
            DX1 = Int32.Parse(linesSplit[11]);
            DX2 = Int32.Parse(linesSplit[12]);
            WIDTHOFCRYST = Int32.Parse(linesSplit[13]);
        }

        static void ReadCoordinateFile(string path)
        {
            int i = 0;
            string[] linesSplit;
            string[] jumpers;
            int jumperNumber;

            foreach (string line in ReadAllLinesFromFile(path))
            {
                CrystCoord temp = new CrystCoord();
                linesSplit = line.Split('\t');
                jumpers = linesSplit[2].Split('F');
                temp.x = Int32.Parse(linesSplit[0]);
                temp.y = Int32.Parse(linesSplit[1]);

                if (jumpers[0] == "-")
                {
                    temp.f1 = false;
                    temp.f2 = false;
                    temp.f3 = false;
                }
                else
                {
                    switch (jumpers.Length)
                    {
                        case 1:
                            {
                                temp.f1 = false;
                                temp.f2 = false;
                                temp.f3 = false;
                                break;
                            }
                        case 2:
                            {
                                if (Int32.TryParse(jumpers[1], out jumperNumber))
                                {
                                    switch (jumperNumber)
                                    {
                                        case 1:
                                            {
                                                temp.f1 = true;
                                                break;
                                            }
                                        case 2:
                                            {
                                                temp.f2 = true;
                                                break;
                                            }
                                        case 3:
                                            {
                                                temp.f3 = true;
                                                break;
                                            }
                                        default:
                                            {
                                                temp.f1 = false;
                                                temp.f2 = false;
                                                temp.f3 = false;
                                                break;
                                            }
                                    }
                                }
                                else
                                {
                                    temp.f1 = false;
                                    temp.f2 = false;
                                    temp.f3 = false;
                                }
                                break;
                            }
                        case 3:
                            {
                                if (Int32.TryParse(jumpers[1], out jumperNumber))
                                {
                                    switch (jumperNumber)
                                    {
                                        case 1:
                                            {
                                                temp.f1 = true;
                                                break;
                                            }
                                        case 2:
                                            {
                                                temp.f2 = true;
                                                break;
                                            }
                                        case 3:
                                            {
                                                temp.f3 = true;
                                                break;
                                            }
                                        default:
                                            {
                                                temp.f1 = false;
                                                temp.f2 = false;
                                                temp.f3 = false;
                                                break;
                                            }
                                    }
                                }
                                else
                                {
                                    temp.f1 = false;
                                    temp.f2 = false;
                                    temp.f3 = false;
                                }

                                if (Int32.TryParse(jumpers[2], out jumperNumber))
                                {
                                    switch (jumperNumber)
                                    {
                                        case 2:
                                            {
                                                temp.f2 = true;
                                                break;
                                            }
                                        case 3:
                                            {
                                                temp.f3 = true;
                                                break;
                                            }
                                        default:
                                            {
                                                temp.f2 = false;
                                                temp.f3 = false;
                                                break;
                                            }
                                    }
                                }
                                else
                                {
                                    temp.f2 = false;
                                    temp.f3 = false;
                                }
                                break;
                            }
                        case 4:
                            {
                                if (Int32.TryParse(jumpers[1], out jumperNumber))
                                {
                                    switch (jumperNumber)
                                    {
                                        case 1:
                                            {
                                                temp.f1 = true;
                                                break;
                                            }
                                        case 2:
                                            {
                                                temp.f2 = true;
                                                break;
                                            }
                                        case 3:
                                            {
                                                temp.f3 = true;
                                                break;
                                            }
                                        default:
                                            {
                                                temp.f1 = false;
                                                temp.f2 = false;
                                                temp.f3 = false;
                                                break;
                                            }
                                    }
                                }
                                else
                                {
                                    temp.f1 = false;
                                    temp.f2 = false;
                                    temp.f3 = false;
                                }

                                if (Int32.TryParse(jumpers[2], out jumperNumber))
                                {
                                    switch (jumperNumber)
                                    {
                                        case 2:
                                            {
                                                temp.f2 = true;
                                                break;
                                            }
                                        case 3:
                                            {
                                                temp.f3 = true;
                                                break;
                                            }
                                        default:
                                            {
                                                temp.f2 = false;
                                                temp.f3 = false;
                                                break;
                                            }
                                    }
                                }
                                else
                                {
                                    temp.f2 = false;
                                    temp.f3 = false;
                                }

                                if (Int32.TryParse(jumpers[3], out jumperNumber))
                                {
                                    switch (jumperNumber)
                                    {
                                        case 3:
                                            {
                                                temp.f3 = true;
                                                break;
                                            }
                                        default:
                                            {
                                                temp.f3 = false;
                                                break;
                                            }
                                    }
                                }
                                else
                                {
                                    temp.f3 = false;
                                }
                                break;
                            }
                    }
                }
                crystCoords.Add(temp);
                i++;
            }
            //crystCoords.Reverse();
        }

        static void ColumnProcessing(string path)
        {
            bool laserNecessary = false;
            bool firstPassage = false;
            Direction direction = Direction.TOP;
            int countRows = 0;
            int currentCryst = countCryst;

            CrystCoord crystCoord;

            crystCoord = crystCoords[currentCryst];
            x = crystCoord.x;
            y = crystCoord.y;
            Delay(0.3);
            DateTime dateTime1 = DateTime.Now;
            //going to the start position
            GoTo(x, y + H1 + ALLOWEDH + H2);
            y += H1 + ALLOWEDH + H2;
            GoTo(x + DX1, y);
            x += DX1;
            direction = Direction.TOP;

            firstPassage = true;
            while (countRows != 3)
            {
                switch (direction)
                {
                    case Direction.TOP:
                        {
                            while (y != HBORDER2)
                            {
                                if (firstPassage)
                                    countCryst++;
                                currentCryst++;
                                crystCoord = crystCoords[currentCryst - 1];
                                switch (countRows)
                                {
                                    case 0:
                                        {
                                            if (crystCoord.f1)
                                                laserNecessary = true;
                                            break;
                                        }
                                    case 1:
                                        {
                                            if (crystCoord.f2)
                                                laserNecessary = true;
                                            break;
                                        }
                                    case 2:
                                        {
                                            if (crystCoord.f3)
                                                laserNecessary = true;
                                            break;
                                        }
                                }
                                FromDownToTop(x, ref y, laserNecessary);
                                laserNecessary = false;
                                if (y != HBORDER2)
                                {
                                    GoTo(x, y - HDISTBETWCRYST);
                                    y -= HDISTBETWCRYST;
                                }
                                else
                                {
                                    countRows++;
                                    if (countRows == 1)
                                        firstPassage = false;
                                    if (countRows != 3)
                                    {
                                        GoTo(x + DX2, y);
                                        x += DX2;
                                    }
                                    direction = Direction.DOWN;
                                    break;
                                }
                            }
                            break;
                        }
                    case Direction.DOWN:
                        {
                            while (y != HBORDER1)
                            {
                                crystCoord = crystCoords[currentCryst - 1];
                                currentCryst--;
                                switch (countRows)
                                {
                                    case 0:
                                        {
                                            if (crystCoord.f1)
                                                laserNecessary = true;
                                            break;
                                        }
                                    case 1:
                                        {
                                            if (crystCoord.f2)
                                                laserNecessary = true;
                                            break;
                                        }
                                    case 2:
                                        {
                                            if (crystCoord.f3)
                                                laserNecessary = true;
                                            break;
                                        }
                                }
                                FromTopToDown(x, ref y, laserNecessary);
                                laserNecessary = false;
                                if (y != HBORDER1)
                                {
                                    GoTo(x, y + HDISTBETWCRYST);
                                    y += HDISTBETWCRYST;
                                }
                                else
                                {
                                    countRows++;
                                    GoTo(x + DX1, y);
                                    x += DX2;
                                    direction = Direction.TOP;
                                    break;
                                }
                            }
                            break;
                        }
                }
            }
            DateTime dateTime2 = DateTime.Now;
            if (dateTime2.Subtract(dateTime1).TotalSeconds > 10)
                Autofocus();
        }

    }
}