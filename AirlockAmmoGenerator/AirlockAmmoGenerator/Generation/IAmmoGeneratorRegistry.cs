namespace AirlockAmmoGenerator.Generation
{
    public interface IAmmoGeneratorRegistry
    {
        IAmmoGenerator Get(AmmoType ammoType);
        IAmmoGenerator Set(AmmoType ammoType, IAmmoGenerator generator);
    }
}