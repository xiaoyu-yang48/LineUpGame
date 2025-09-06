using System;

namespace LineUp
{
    public static class ConsoleGame
    {
        public static void Start()
        {
            Console.WriteLine("Welcome to Line Up!");
            // Game logic goes here
        }

        public class GameBoard
        {
            public int Rows { get; }
            public int Cols { get; }

            public static (int rows, int cols) SetBoardSize()
            {
                const int minRows = 6;
                const int minCols = 7;
                int rows = 0;
                int cols = 0;

                while (true)
                {
                    try
                    {
                        Console.WriteLine($"Please enter your board rows: (>= {minRows})");
                        rows = int.Parse(Console.ReadLine());
                        Console.WriteLine($"Please enter your board columns: (>= {minCols}), and rows <= columns");
                        cols = int.Parse(Console.ReadLine());

                        if (rows < minRows)
                            throw new ArgumentOutOfRangeException($"Rows must be >= {minRows}");
                        if (cols < minCols)
                            throw new ArgumentOutOfRangeException($"Columns must be >= {minCols}");
                        if (rows > cols)
                            throw new ArgumentOutOfRangeException("Rows cannot exceed columns.");

                        break;
                    }
                    catch (ArgumentNullException)
                    {
                        Console.WriteLine("Your input was null.");
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Your input was not a valid integer.");
                    }
                    catch (OverflowException)
                    {
                        Console.WriteLine("Your number is too big or small");
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Invalid input");
                    }
                }

                return (rows, cols);
            }
        }
        
    }
}