namespace TimeSheet.Application.Structs
{
    /// <summary>
    /// Information about a console command
    /// </summary>
    /// <param name="Name"> Command name </param>
    /// <param name="Help"> Help description </param>
    /// <param name="Order"> Order in a help chapter </param>
    internal record ConsoleCommand(string Name, string Help, int Order) : IComparable<ConsoleCommand>
    {
        /// <summary>
        /// Command name
        /// </summary>
        internal string Name { get; set; } = Name;

        /// <summary>
        /// Help description
        /// </summary>
        internal string Help { get; set; } = Help;

        /// <summary>
        /// 
        /// </summary>
        internal int Order { get; set; } = Order;

        /// <summary>
        /// Compare two console commands by order
        /// </summary>
        /// <param name="other"> Second instance </param>
        /// <returns> Comparison result </returns>
        public int CompareTo(ConsoleCommand? other)
        {
            return Order.CompareTo(other?.Order);
        }
    }
}
