using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PictureToLaser
{
    internal class NcToGcode:GCodeConverter
    {
        private readonly Options _arg;

        public NcToGcode(Options arg) : base(arg)
        {
            _arg = arg;
        }

        public object Process(out Queue<AbstractCommand> result)
        {
            var source = File.ReadAllLines(_arg.FilePath).SelectMany(v => v.Split(' ')).ToList();
            var commands = ConvertFromNc(source);
            
            result = new Queue<AbstractCommand>();
    
            Header(commands).Requeue(result);
            commands.Requeue(result);
            Footer().Requeue(result);
            
            return 1;
        }

        private Queue<AbstractCommand> ConvertFromNc(IEnumerable<String> source)
        {
            var commands = new List<AbstractCommand>();

            double? lastPower = null;
            AbstractCommand lastCommand = null;
            
            foreach (var line in source)
            {
                switch (line)
                {
                    case "G90":
                        continue;
                    case "G0":
                        commands.Add(lastCommand = new Move() { Linear = true });
                        continue;
                    case "G1":
                        commands.Add(lastCommand = new Move() { Linear = false });
                        continue;
                    case "G2":
                        commands.Add(lastCommand = new Move { ArcClockwise = true});
                        continue;
                    case "G3":
                        commands.Add(lastCommand = new Move { ArcCounterClockwise = true});
                        continue;
                    case "M3":
                        commands.Add(lastCommand = new SetLaserPower(lastPower ?? -1));
                        continue;
                    case "M5":
                        commands.Add(lastCommand = new SetLaserPower(0));
                        continue;
                    default:
                        var value = double.Parse(line.Substring(1), new NumberFormatInfo(){NumberDecimalSeparator = "."});
                        if (line.StartsWith("S"))
                        {
                            lastPower = value;
                            if (lastCommand is SetLaserPower power && power.Power == -1)
                            {
                                power.Power = value;
                            }
                            else if (lastCommand is Move move)
                            {
                                var newCommand = new SetLaserPower(value);
                                commands.Insert(commands.IndexOf(lastCommand), newCommand);
                            }
                            else
                            {
                                commands.Add(lastCommand = new SetLaserPower(value));
                            }
                        }
                        else if (line.StartsWith("I"))
                        {
                            if (lastCommand is Move move && !move.NewI.HasValue)
                            {
                                move.NewI = value;
                            }
                            else
                            {
                                throw new NotSupportedException();
                            }
                        }
                        else if (line.StartsWith("J"))
                        {
                            if (lastCommand is Move move && !move.NewJ.HasValue)
                            {
                                move.NewJ = value;
                            }
                            else
                            {
                                throw new NotSupportedException();
                            }
                        }
                        else if (line.StartsWith("X"))
                        {
                            if (lastCommand is Move move && !move.NewX.HasValue)
                            {
                                move.NewX = value;
                            }
                            else
                            {
                                commands.Add(lastCommand = new Move {NewX = value});
                            }
                        }
                        else if (line.StartsWith("Y"))
                        {
                            if (lastCommand is Move move && !move.NewY.HasValue)
                            {
                                move.NewY = value;
                            }
                            else
                            {
                                commands.Add(lastCommand = new Move {NewY = value});
                            }
                        }
                        else if (line.StartsWith("F"))
                        {
                            if (lastCommand is Move move && !move.Rate.HasValue)
                            {
                                move.Rate = (int) value;
                            }
                            else
                            {
                                commands.Add(lastCommand = new Move {Rate = (int) value});
                            }
                        }
                        else
                        {
                            throw new NotSupportedException(line);
                        }

                        break;
                }
            }
            
            var result = new Queue<AbstractCommand>();
            commands.ForEach(result.Enqueue);
            return result;
        }
    }
}