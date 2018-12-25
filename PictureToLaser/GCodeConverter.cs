using System;
using System.Collections.Generic;
using System.Linq;

namespace PictureToLaser
{
    internal abstract class GCodeConverter
    {
        private readonly Options _arg;

        public GCodeConverter(Options arg)
        {
            _arg = arg;
        }
        
        protected Queue<AbstractCommand> Header(Queue<AbstractCommand> imageToCommands)
        {
            var details = CalculateTravelDistance(imageToCommands);

            var totalTime = new TimeSpan(0, 0, (int) (details.TravelDistance / _arg.TravelRate * 60));
    
            var cmdRate = (int)(_arg.TravelRate / _arg.ResX * 2 / 60);

            var widthCm = details.MaxX - details.MinX;
            var heightCm = details.MaxY - details.MinY;
            var laserMin = details.LaserMin;
            var laserMax = details.LaserMax;
            
            var commands = new Queue<AbstractCommand>();
            commands.Enqueue(new Comment($"Size in cm X={widthCm}, Y={heightCm}"));
            commands.Enqueue(new Comment($"Speed is {_arg.TravelRate} mm/min, {_arg.ResX} mm/pix => {cmdRate} lines/sec"));
            commands.Enqueue(new Comment($"Power is {laserMin} to {laserMax} (" + laserMin / 255.0 * 100 + "%-" +
                                         laserMax / 255.0 * 100 + "%)"));

            commands.Enqueue(new Comment($"Estimated engraving time: {totalTime}"));
            
            Console.WriteLine(String.Join("\n", commands));

            commands.Enqueue(new MillimeterUnitsCommand());
            
            commands.Enqueue(new BedLevelingCommand(false));

            commands.Enqueue(new Move {NewX = details.MinX, NewY = details.MinY, Rate = _arg.TravelRate});

            commands.Enqueue(new SetLaserPower(0) {Comment = "Turn laser pwm off"});
            commands.Enqueue(new TurnLaserOn());

            commands.Enqueue(new SetFanPower(255));
            
            // Draw rect around area;
            commands.Enqueue(new SetLaserPower(1));
            commands.Enqueue(new Move {NewX = details.MaxX});
            commands.Enqueue(new Move {NewY = details.MaxY});
            commands.Enqueue(new Move {NewX = details.MinX});
            commands.Enqueue(new Move {NewY = details.MinY});

            commands.Enqueue(new Pause());

            commands.Enqueue(new SetLaserPower(0));

            return commands;
        }

        protected Queue<AbstractCommand> Footer()
        {
            var commands = new Queue<AbstractCommand>();
            commands.Enqueue(new DisableLaserPower());
            commands.Enqueue(new TurnLaserOff {Comment = "Turn laser power off"});

            commands.Enqueue(new SetFanPower(0));

            commands.Enqueue(new BedLevelingCommand(true));
            commands.Enqueue(new Move {NewX = 0, NewY = 0, Rate = _arg.TravelRate, Comment = "Go home"});

            return commands;
        }
        
        private static MovementDetails CalculateTravelDistance(Queue<AbstractCommand> commands)
        {
            var imageCommands = commands.ToList();

            var result = new MovementDetails();
            
            var currentX = 0.0;
            var currentY = 0.0;
            foreach (var item in imageCommands)
            {
                if (item is Move move)
                {
                    if (move.NewX.HasValue)
                    {
                        result.TravelDistance += Math.Abs(move.NewX.Value - currentX);
                        currentX = move.NewX.Value;

                        result.MinX = Math.Min(result.MinX, move.NewX.Value);
                        result.MaxX = Math.Max(result.MaxX, move.NewX.Value);
                    }

                    if (move.NewY.HasValue)
                    {
                        result.TravelDistance += Math.Abs(move.NewY.Value - currentY);
                        currentY = move.NewY.Value;
                        
                        result.MinY = Math.Min(result.MinY, move.NewY.Value);
                        result.MaxY = Math.Max(result.MaxY, move.NewY.Value);
                    }
                }

                if (item is SetLaserPower power)
                {
                    result.LaserMin = (int) Math.Min(power.Power, result.LaserMin);
                    result.LaserMax = (int) Math.Max(power.Power, result.LaserMax);
                }
            }

            return result;
        }
    }
}