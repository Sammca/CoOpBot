using Discord;

namespace CoOpBot
{
    // Define array holding random team assignment information [user, teamNumber]
    public class TeamAssignment
    {
        public IUser user { get; set; }
        public int teamNumber { get; set; }
    }
}
