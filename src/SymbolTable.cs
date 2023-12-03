namespace OCompiler.Codegen;

public class SymbolTable<TSymbol>
{
    private class Scope
    {
        public Dictionary<string, TSymbol> Symbols { get; } = new Dictionary<string, TSymbol>();
    }

    private readonly Stack<Scope> scopes = new Stack<Scope>();

    public SymbolTable()
    {
        EnterScope();
    }

    public void EnterScope()
    {
        scopes.Push(new Scope());
    }

    public void ExitScope()
    {
        if (scopes.Count == 0)
        {
            throw new InvalidOperationException("No scope to exit.");
        }

        scopes.Pop();
    }

    public void DefineSymbol(string name, TSymbol symbol)
    {
        if (scopes.Count == 0)
        {
            throw new InvalidOperationException("No scope to define a symbol in.");
        }

        scopes.Peek().Symbols[name] = symbol;
    }

    public TSymbol FindSymbol(string name)
    {
        foreach (var scope in scopes)
        {
            if (scope.Symbols.TryGetValue(name, out TSymbol symbol))
            {
                return symbol;
            }
        }

        throw new KeyNotFoundException($"Symbol '{name}' not found in any scope.");
    }
}