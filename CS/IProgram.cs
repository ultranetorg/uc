namespace Uccs;

public interface IProgram
{
	Thread CreateThread(Action action);

}
