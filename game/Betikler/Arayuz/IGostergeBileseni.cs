namespace Krallik.Arayuz;

/// <summary>
/// Yönetim, Halk, Para ve Ordu çubuklarının ortak sunum sözleşmesi.
/// Yeni bir gösterge bu arayüzü uyguladığında üst panel tarafından otomatik bulunur.
/// </summary>
public interface IGostergeBileseni
{
    string Anahtar { get; }
    void DegeriGoster(int deger);
}
