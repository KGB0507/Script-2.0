using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Spels.AutomationTools.Engine;
using Spels.AutomationTools.Imaging;
using Spels.AutomationTools.Miscellaneous;
using Spels.AutomationTools.Scanners;
using Spels.AutomationTools.Utility;
using System.Collections.Generic;
using Spels.AutomationTools.Lasers;
using System.IO;
using System;

namespace Scripts
{
    public class TestScript : BasicScript
    {
        [DeviceRequest]
        public LayeredDrawing LayeredDrawing { get; set; }
        public double Step { get; set; } = 10d;
        
        [DeviceRequest]
        public LaserFacility LaserFacility { get; set; } 
        [DeviceRequest]
        public Motion3D Motion3D {get;set;}
        
        
        public string PathCoordinates { get; set; } = @"D:\ИСПЫТАНИЯ\2023\резка test script\CrystCoordinates.txt";
        public string PathParameters { get; set; } = @"D:\ИСПЫТАНИЯ\2023\резка test script\CrystParameters.txt";

        //current coordinates of the laser:
        public double x = 0;
        public double y = 0;
        public double z = 0;

        public int countCryst = 0;

        //Crystal Parameters:
        public int NUMOFCOLS;
        public int HDISTBETWCRYST;
        public int WDISTBETWCRYST;
        public int ALLOWEDH;
        public int ALLOWEDW;
        public int HBORDER1;
        public int HBORDER2;
        public int WBORDER1;
        public int WBORDER2;
        public int H1;
        public int H2;
        public int DX1;
        public int DX2;
        public int WIDTHOFCRYST;

        //structure (class) for keeping crystal's properties:
        
        public struct CrystCoord
        {
            public int x;
            public int y;
            public bool f1;
            public bool f2;
            public bool f3; 
        }
            
        public List<List<CrystCoord>> colsOfCrysts = new List<List<CrystCoord>>();

        enum Direction
        {
            TOP = 1,
            DOWN = 2
        };
        void LaserOn()
        {
            LaserFacility.Shooter.EnableShooting = true;   
        }
              
        void LaserOff()
        {
            LaserFacility.Shooter.EnableShooting = false;
        }
        
        async Task GoTo(double xNew, double yNew)
        {
        
         await Motion3D.GoTo(XYZMask.XY,xNew,yNew,0).ConfigureAwait(false);   
            
        }
        
        async Task<double> FromTopToDown(double x, double y, bool laserNecessary)
        {
            if (laserNecessary == true)
            {
                y += H2;
                await GoTo(x, y);
                LaserOn();
                y += ALLOWEDH;
                await GoTo(x, y);
                LaserOff();
                y += H1;
                await GoTo(x, y);
            }
            else
            {
                y += H2 + ALLOWEDH + H1;
                await GoTo(x, y);
            }
            return y;
        }
        async Task<double> FromDownToTop(double x, double y, bool laserNecessary)
        {
            if (laserNecessary == true)
            {
                y -= H1;
                await GoTo(x, y);
                LaserOn();
                y -= ALLOWEDH;
                await GoTo(x, y);
                LaserOff();
                y -= H2;
                await GoTo(x, y);
            }
            else
            {
                y -= H1 + ALLOWEDH + H2;
                await GoTo(x, y);
            }
            return y;
        }
        
        
        IEnumerable<string> ReadAllLinesFromFile(string path)
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
        
        void SetCrystParameters(string path)
        {
            const int ParamCount = 14;
            string[] linesSplit = new string[ParamCount];
            int i = 0;
            foreach (string line in ReadAllLinesFromFile(path))
            {
                TextOutput.WriteLine($"{line}");
                linesSplit[i] = line.Split('\t')[1];
                i++;
            }
            NUMOFCOLS = int.Parse(linesSplit[0]);
            HDISTBETWCRYST = int.Parse(linesSplit[1]);
            WDISTBETWCRYST = int.Parse(linesSplit[2]);
            ALLOWEDH = int.Parse(linesSplit[3]);
            ALLOWEDW = int.Parse(linesSplit[4]);
            HBORDER1 = int.Parse(linesSplit[5]);
            HBORDER2 = int.Parse(linesSplit[6]);
            WBORDER1 = int.Parse(linesSplit[7]);
            WBORDER2 = int.Parse(linesSplit[8]);
            H1 = int.Parse(linesSplit[9]);
            H2 = int.Parse(linesSplit[10]);
            DX1 = int.Parse(linesSplit[11]);
            DX2 = int.Parse(linesSplit[12]);
            WIDTHOFCRYST = int.Parse(linesSplit[13]);
        }
        
        void ReadCoordinateFile(string path)
        {
            int i = 0;
            int numOfCol = 1;
            double tempX = 0;
            string[] linesSplit;
            string[] jumpers;
            int jumperNumber;
            List<CrystCoord> colOfCrysts = new List<CrystCoord>();

            foreach (string line in ReadAllLinesFromFile(path))
            {
                CrystCoord temp = new CrystCoord();
                linesSplit = line.Split('\t');
                jumpers = linesSplit[2].Split('F');
                temp.x = int.Parse(linesSplit[0]);
                temp.y = int.Parse(linesSplit[1]);
                if(temp.x > tempX + WIDTHOFCRYST)
                {
                numOfCol++;
                }
                tempX = temp.x;

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
                                if (int.TryParse(jumpers[1], out jumperNumber))
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
                                if (int.TryParse(jumpers[1], out jumperNumber))
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

                                if (int.TryParse(jumpers[2], out jumperNumber))
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
                                if (int.TryParse(jumpers[1], out jumperNumber))
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

                                if (int.TryParse(jumpers[2], out jumperNumber))
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

                                if (int.TryParse(jumpers[3], out jumperNumber))
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
                colOfCrysts.Add(temp);
                if(numOfCol > colsOfCrysts.Count)
                {
                colsOfCrysts.Add(colOfCrysts);
                }
                i++;
            }
            
        }
        
        async Task ColumnProcessing(int numOfCol)
        {
            bool laserNecessary = false;
            bool firstPassage = false;
            
            Direction direction = Direction.TOP;
            int countRows = 0;
            int currentCryst = countCryst;

            CrystCoord crystCoord;
            TextOutput.WriteLine($"crystCoord.Count = {colsOfCrysts[numOfCol].Count}");
            Checkpoint(417); crystCoord = colsOfCrysts[numOfCol][currentCryst];
            x = crystCoord.x;
            y = crystCoord.y;
            
            //await Task.Delay(300);
            
            DateTime dateTime1 = DateTime.Now;
            //going to the start position
            y += H1 + ALLOWEDH + H2;
            await GoTo(x, y);
            x += DX1;
            await GoTo(x, y);
            direction = Direction.TOP;
            
            firstPassage = true;
            bool runningLoop = true;
            do
            {
                TextOutput.WriteLine($"currentCryst = {currentCryst}; direction = {direction}; X = {x}; Y = {y}; ");
                switch (direction)
                {
                    case Direction.TOP:
                        {
                            while (y != HBORDER2 && colsOfCrysts[numOfCol].Count )/////////////
                            {
                                if (firstPassage)
                                {
                                   countCryst++;
                                }
                                currentCryst++;
                                TextOutput.WriteLine($"currentCryst = {currentCryst}; X = {x}; Y = {y}; ");
                                Checkpoint(446); crystCoord = colsOfCrysts[numOfCol][currentCryst - 1];
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
                                    default:
                                        {
                                            throw new ArgumentOutOfRangeException($"{countRows}","WTF??");
                                        }
                                }
                                TextOutput.WriteLine($"currentCryst = {currentCryst}; change dir = {y == HBORDER2}; laserNecessary = {laserNecessary}");
                                y = await FromDownToTop(x, y, laserNecessary);
                                laserNecessary = false;
                                if (y != HBORDER2 && colsOfCrysts[numOfCol].Count)////////////////////////////////
                                {
                                    y -= HDISTBETWCRYST;
                                    await GoTo(x, y);
                                    
                                }
                                else
                                {
                                    countRows++;
                                    if (countRows == 1)
                                        firstPassage = false;
                                    if (countRows != 3)
                                    {   
                                        x += DX2;
                                        await GoTo(x, y);
                                        
                                    }
                                    direction = Direction.DOWN;
                                    break;
                                }
                                TextOutput.WriteLine($"currentCryst = {currentCryst}; countRows = {countRows}");
                                await Barrier.EnsureContinuationPermitted().ConfigureAwait(false);
                            }
                            break;
                        }
                    case Direction.DOWN:
                        {
                            while (y != HBORDER1 && colsOfCrysts[numOfCol][countCryst])///////////////////
                            {
                                Checkpoint(506); crystCoord = colsOfCrysts[numOfCol][currentCryst - 1];
                                currentCryst--;
                                TextOutput.WriteLine($"currentCryst = {currentCryst}; X = {x}; Y = {y};");
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
                                    default:
                                        {
                                            throw new ArgumentOutOfRangeException($"{countRows}","WTF??");
                                        }
                                }
                                y = await FromTopToDown(x, y, laserNecessary);
                                TextOutput.WriteLine($"currentCryst = {currentCryst}; change dir = {y == HBORDER1}");
                                laserNecessary = false;
                                if (y != HBORDER1 && colsOfCrysts[numOfCol][countCryst])/////////////////////
                                {
                                   y += HDISTBETWCRYST;
                                   await GoTo(x, y);
                                }
                                else
                                {
                                    countRows++;
                                    if (countRows != 3)
                                    {   
                                        x += DX2;
                                        await GoTo(x, y);
                                        
                                    }
                                    direction = Direction.TOP;
                                    break;
                                }
                                TextOutput.WriteLine($"currentCryst = {currentCryst}; X = {x}; Y = {y}; countRows = {countRows}");
                                await Barrier.EnsureContinuationPermitted().ConfigureAwait(false);
                            }
                            break;
                        }
                }
                await Barrier.EnsureContinuationPermitted().ConfigureAwait(false);
                await Task.Delay(1000).ConfigureAwait(false);
                
                runningLoop = countRows != 3;
                
            }while (runningLoop);
            DateTime dateTime2 = DateTime.Now;
            TextOutput.WriteLine(dateTime2);
            //if (dateTime2.Subtract(dateTime1).TotalSeconds > 10)
            //Autofocus();    
        }

        protected override async Task WorkingFunction()
        {   
            SetCrystParameters(PathParameters);
            ReadCoordinateFile(PathCoordinates);
            countCryst = 0;
            try
            {
                for (int k = 0; k < NUMOFCOLS ; k++)
                {
                    TextOutput.WriteLine($"Start column {k+1}");
                    await ColumnProcessing(k);
                    TextOutput.WriteLine($"Finish column {k+1}");
                }
            }
            finally
            {            
                (LaserFacility.Attenuator as StepperMotorAttenuator).CalibrateToZero().Forget(); 
            }

        }

        }
      
    }    

     //await Barrier.EnsureContinuationPermitted().ConfigureAwait(false);
