namespace CodeExample;

public abstract class CommandHandler<TIn, TOut> {
    public abstract Task<TOut> HandleRequestAsync(TIn input);
}

public class TestCommandHandler : CommandHandler<string, int[]> {
    public override Task<int[]> HandleRequestAsync(string input) => throw new NotImplementedException();
}