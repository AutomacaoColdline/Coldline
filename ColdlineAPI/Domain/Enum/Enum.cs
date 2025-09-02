namespace ColdlineAPI.Domain.Enum
{
    public enum MachineStatus
    {
        WaitingProduction = 1,
        InProgress = 2,
        InOcurrence = 3,
        InRework = 4,
        Finished = 5,
        Stop = 6,
    }
    public enum NoteType
    {
        Passwords = 1,
        DockerCommands = 2,
        LinuxCommands = 3,
        WindowsCommands = 4,
        Notices = 5,
        Reminders = 6
    }
}