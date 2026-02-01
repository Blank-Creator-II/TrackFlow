using Microsoft.Win32.TaskScheduler;

public static class TaskScheduleHelper
{
    // Creates or updates a scheduled task
    public static void CreateOrUpdateTask(string taskName, string executablePath, string? arguments = null, string? workingDirectory = null, Trigger trigger = null!, bool runAsHighest = false)
    {
        using TaskService taskService = new TaskService();

        TaskDefinition task = taskService.NewTask();
        task.RegistrationInfo.Description = taskName;

        // Trigger
        trigger.EndBoundary = trigger.StartBoundary.AddDays(1);
        task.Settings.DeleteExpiredTaskAfter = TimeSpan.FromSeconds(1);
        task.Triggers.Add(trigger);

        // Action
        task.Actions.Add(new ExecAction(
            executablePath,
            arguments,
            workingDirectory
        ));

        // Settings
        task.Settings.StartWhenAvailable = true;
        task.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;
        task.Settings.DisallowStartIfOnBatteries = false;
        task.Settings.StopIfGoingOnBatteries = false;

        if (runAsHighest){task.Principal.RunLevel = TaskRunLevel.Highest;}

        // Register (this will UPDATE if it already exists)
        taskService.RootFolder.RegisterTaskDefinition(
            taskName,
            task,
            TaskCreation.CreateOrUpdate,
            null,
            null,
            TaskLogonType.InteractiveToken
        );
    }

    // Deletes a scheduled task if it exists
    public static bool DeleteTask(string taskName)
    {
        using TaskService taskService = new TaskService();

        if (TaskExists(taskName))
        {
            taskService.RootFolder.DeleteTask(taskName, false);
            return true;
        }
        
        return false;
    }

    // Checks whether a scheduled task exists
    public static bool TaskExists(string taskName)
    {
        using TaskService taskService = new TaskService();
        return taskService.GetTask(taskName) != null;
    }
}
