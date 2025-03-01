using AutoMapper;

namespace AzureCosmos.CRUD.WebAPI.AutoMapper
{
  public static class AutoMapperConfig
  {
    private static readonly Lazy<MapperConfiguration> LazyConfiguration =
      new Lazy<MapperConfiguration>(() =>
      {
        return new MapperConfiguration(cfg =>
        {
          cfg.AllowNullCollections = true;
          cfg.AddProfile<BookMapperProfile>();
          cfg.DisableConstructorMapping();
        });
      });

    private static readonly Lazy<IMapper> LazyMapper =
      new Lazy<IMapper>(LazyConfiguration.Value.CreateMapper);
  }
}
