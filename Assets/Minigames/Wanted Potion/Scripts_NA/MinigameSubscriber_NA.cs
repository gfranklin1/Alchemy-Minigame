public interface MinigameSubscriber_NA
{
    /// <summary>
    /// Called after the initial countdown, when the minigame actually begins
    /// </summary>
    public abstract void OnMinigameStart();

    /// <summary>
    /// Called after the timer expires. 
    /// The minigame will NOT end after this. 
    /// </summary>
    public abstract void OnTimerEnd();
}