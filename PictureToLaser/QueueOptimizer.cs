using System;
using System.Collections.Generic;

namespace PictureToLaser
{
    internal static class QueueOptimizer
    {
        public static Queue<AbstractCommand> Optimize(Queue<AbstractCommand> commands)
        {
            bool lengthChanged = true;
            while (lengthChanged)
            {
                var oldLength = commands.Count;
                commands = OptimizeQueueLaserPower(commands);
                commands = OptimizeQueueLineMoves(commands);
                commands = OptimizeQueueMoves(commands);
                commands = OptimizeQueueStatuses(commands);
                lengthChanged = commands.Count < oldLength;
                Console.WriteLine($"Optimization, command count {oldLength} -> {commands.Count}");
            }

            return commands;
        }

        private static Queue<AbstractCommand> OptimizeQueueLaserPower(Queue<AbstractCommand> source)
        {
            var target = new Queue<AbstractCommand>();

            string lastPower = null;

            while (source.Count > 0)
            {
                var current = source.Dequeue();

                switch (current)
                {
                    case SetLaserPower power:
                    {
                        var newPower = power.Power;
                        if (newPower.MyInt() == lastPower)
                        {
                            continue;
                        }

                        lastPower = newPower.MyInt();
                        break;
                    }
                    case DisableLaserPower _:
                    case TurnLaserOn _:
                    case TurnLaserOff _:
                        lastPower = null;
                        break;
                }

                target.Enqueue(current);
            }

            return target;
        }

        private static Queue<AbstractCommand> OptimizeQueueLineMoves(Queue<AbstractCommand> source)
        {
            var target = new Queue<AbstractCommand>();

            Move move1 = null, move2 = null;

            while (source.Count > 0)
            {
                var current = source.Dequeue();

                if (current is Move move)
                {
                    if (move1 == null)
                    {
                        move1 = move;
                    }
                    else
                    {
                        if (move2 == null)
                        {
                            move2 = move;
                        }
                        else
                        {
                            bool onTheSameLine;
                            if (move1.NewX != null && move1.NewY == null && move1.Rate == null &&
                                move2.NewX != null && move2.NewY == null && move2.Rate == null &&
                                move.NewX != null && move.NewY == null && move.Rate == null)
                            {
                                onTheSameLine = move1.NewX < move2.NewX && move2.NewX < move.NewX ||
                                                move1.NewX > move2.NewX && move2.NewX > move.NewX;
                            }
                            else
                            {
                                if (move1.NewY != null && move1.NewX == null && move1.Rate == null &&
                                    move2.NewY != null && move2.NewX == null && move2.Rate == null &&
                                    move.NewY != null && move.NewX == null && move.Rate == null)
                                {
                                    onTheSameLine = move1.NewY < move2.NewY && move2.NewY < move.NewY ||
                                                    move1.NewY > move2.NewY && move2.NewY > move.NewY;
                                }
                                else
                                {
                                    onTheSameLine = false;
                                }
                            }

                            if (onTheSameLine)
                            {
                                continue;
                            }

                            move1 = null;
                            move2 = null;
                        }
                    }
                }
                else if (!(current is Comment) && !(current is Status))
                {
                    move1 = null;
                    move2 = null;
                }

                target.Enqueue(current);
            }

            return target;
        }

        private static Queue<AbstractCommand> OptimizeQueueStatuses(Queue<AbstractCommand> source)
        {
            var target = new Queue<AbstractCommand>();

            Status lastStatus = null;

            while (source.Count > 0)
            {
                var current = source.Dequeue();

                if (current is Status s)
                {
                    if (lastStatus == null)
                    {
                        lastStatus = s;
                    }
                    else
                    {
                        lastStatus.Text = s.Text;
                        continue;
                    }
                }
                else
                {
                    lastStatus = null;
                }

                target.Enqueue(current);
            }

            return target;
        }


        private static Queue<AbstractCommand> OptimizeQueueMoves(Queue<AbstractCommand> source)
        {
            var target = new List<AbstractCommand>();

            Move lastX = null;
            Move lastY = null;

            bool anyRemoved = false;
            
            while (source.Count > 0)
            {
                var current = source.Dequeue();

                if (current is Move move)
                {
                    if (move.NewX.HasValue && !move.NewY.HasValue && !move.Rate.HasValue &&
                        move.NewX.Value.MyRound() == lastX?.NewX?.MyRound())
                    {
                        target.Remove(lastX);
                    }
                        
                    if (move.NewY.HasValue && !move.NewX.HasValue && !move.Rate.HasValue &&
                        move.NewY.Value.MyRound() == lastY?.NewY?.MyRound())
                    {
                        target.Remove(lastY);
                    }

                    if (move.NewX.HasValue)
                    {
                        lastX = move;
                    }

                    if (move.NewY.HasValue)
                    {
                        lastY = move;
                    }
                }
                else if (!(current is Comment) && !(current is Status))
                {
                    lastX = null;
                    lastY = null;
                }

                target.Add(current);
            }

            var result = new Queue<AbstractCommand>();
            target.ForEach(result.Enqueue);
            return result;
        }

        private static bool OnTheSameLine(Move move1, Move move2, Move move3)
        {
            if (move1.NewX != null && move1.NewY == null && move1.Rate == null &&
                move2.NewX != null && move2.NewY == null && move2.Rate == null &&
                move3.NewX != null && move3.NewY == null && move3.Rate == null)
            {
                return move1.NewX < move2.NewX && move2.NewX < move3.NewX ||
                       move1.NewX > move2.NewX && move2.NewX > move3.NewX;
            }

            
            if (move1.NewY != null && move1.NewX == null && move1.Rate == null &&
                move2.NewY != null && move2.NewX == null && move2.Rate == null &&
                move3.NewY != null && move3.NewX == null && move3.Rate == null)
            {
                return move1.NewY < move2.NewY && move2.NewY < move3.NewY ||
                       move1.NewY > move2.NewY && move2.NewY > move3.NewY;
            }

            return false;
        }}
}