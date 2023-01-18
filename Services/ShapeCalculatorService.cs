namespace DistributedCache.Services
{
    public abstract class Shape
    {
        public virtual decimal length { get; set; }
        public virtual decimal breadth { get; set; }
        public Shape() { }

        public Shape(decimal length, decimal breadth)
        {
            this.length = length;
            this.breadth = breadth;
        }

        public abstract decimal Area { get; }
    }

    public class Square : Shape
    {
        public Square(decimal length, decimal breadth) : base(length, breadth)
        {
            
        }

        public override decimal Area { get { return this.length * this.breadth;} }
    }

    public class Rectangle : Shape
    {
        public Rectangle(decimal length, decimal breadth) : base(length, breadth)
        {
        }
        public override decimal Area { get { return this.length * this.breadth; } }
    }

    public class Circle : Shape
    {
        private decimal _radius;
        public Circle(decimal radius)
        {
            _radius = radius;
        }
        public override decimal Area { get { return _radius * _radius * (decimal)3.14; } }
    }

    public class ShapeCalculatorService
    {
        public void Calculate()
        {
            var sq = new Square(2,2);
            Console.WriteLine($"Area: {sq.Area}");
        }
    }
}
