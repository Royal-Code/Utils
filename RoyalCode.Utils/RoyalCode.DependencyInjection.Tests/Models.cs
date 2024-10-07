namespace RoyalCode.DependencyInjection.Tests;

public sealed class SimpleService { }

public sealed class SimpleServiceGene<T> { }

public sealed class SimpleServiceGene2<T1, T2> { }

public interface ICommonService { }

public class CommonService : ICommonService { }

public interface ICommonServiceGene<T> { }

public class CommonServiceGene<T> : ICommonServiceGene<T> { }

public interface ICommonServiceGene2<T1, T2> { }

public class CommonServiceGene2<T1, T2> : ICommonServiceGene2<T1, T2> { }

public interface IMultiServiceA { }

public interface IMultiServiceB { }

public class MultiService : IMultiServiceA, IMultiServiceB { }

