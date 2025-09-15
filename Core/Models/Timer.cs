using System;

namespace CronometroRegressivo.Core.Models
{
    /// <summary>
    /// Classe de modelo que representa um temporizador no sistema.
    /// 
    /// Padrões e conceitos utilizados:
    /// - POCO (Plain Old CLR Object): Objeto simples que armazena dados
    /// - Encapsulamento: Propriedades com getters/setters controlados
    /// - Método de extensão: GetTimeSpan() fornece funcionalidade adicional
    /// 
    /// Esta classe é usada como DTO (Data Transfer Object) entre as camadas
    /// </summary>
    public class TimerModel
    {
        /// <summary>
        /// Identificador único do temporizador
        /// Usado para operações CRUD no TimerManager
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome descritivo do temporizador
        /// - Valor padrão: string.Empty (evita null)
        /// - Validação: Deve ser não-nulo/não-vazio (validado no TimerManager)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Duração total do temporizador em segundos
        /// - Validação: Deve ser > 0 (validado no TimerManager)
        /// - Armazenamento: Convertido para TimeSpan quando necessário
        /// </summary>
        public int TimeInSeconds { get; set; }

        /// <summary>
        /// Flag que indica se o temporizador emite alerta sonoro ao terminar
        /// - true: Toca alerta quando o tempo chega a zero
        /// - false: Não emite som ao completar
        /// </summary>
        public bool HasSoundNotification { get; set; }

        /// <summary>
        /// Flag que indica se o temporizador deve reiniciar automaticamente
        /// - true: Reinicia automaticamente ao chegar a zero
        /// - false: Para ao chegar a zero
        /// </summary>
        public bool ShouldRepeat { get; set; } = false;

        /// <summary>
        /// Tempo inicial em segundos (usado para reiniciar o loop)
        /// - Armazena o valor original para repetição
        /// </summary>
        public int InitialSeconds { get; set; }

        /// <summary>
        /// Método auxiliar que converte segundos para TimeSpan
        /// - Fornece uma representação mais rica do tempo
        /// - Facilita formatação e cálculos com tempo
        /// 
        /// Exemplo de uso:
        /// var span = timer.GetTimeSpan();
        /// string formatted = span.ToString(@"hh\:mm\:ss");
        /// </summary>
        /// <returns>TimeSpan representando a duração</returns>
        public TimeSpan GetTimeSpan() => TimeSpan.FromSeconds(TimeInSeconds);
    }
}