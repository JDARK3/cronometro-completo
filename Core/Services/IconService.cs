using System.Drawing;
using System.Drawing.Drawing2D;

namespace CronometroRegressivo.UI.Services
{
    /// <summary>
    /// Serviço estático para geração e manipulação de ícones vetoriais
    /// 
    /// Principais conceitos aplicados:
    /// - Padrão Factory Method: Cria diferentes tipos de ícones através de um método único
    /// - Princípio SRP (Single Responsibility): Classe com única responsabilidade (geração de ícones)
    /// - Encapsulamento: Esconde detalhes de implementação da geração de ícones
    /// - IDisposable: Uso correto de recursos descartáveis (Graphics, Brush, Font)
    /// - Métodos puros: LightenColor não altera estado externo
    /// </summary>
    public static class IconService
    {
        /// <summary>
        /// Cria um bitmap contendo um ícone vetorial baseado no nome solicitado
        /// 
        /// Padrões aplicados:
        /// - Factory Method: Cria objetos (ícones) sem expor a lógica de criação
        /// - Strategy: Diferentes algoritmos para diferentes tipos de ícone
        /// </summary>
        /// <param name="iconName">Nome do ícone a ser gerado (play, pause, reset, menu)</param>
        /// <param name="color">Cor do ícone</param>
        /// <returns>Bitmap com o ícone renderizado</returns>
        public static Bitmap LoadVectorIcon(string iconName, Color color)
        {
            // Cria um novo bitmap de 32x32 pixels (tamanho padrão para ícones)
            var bmp = new Bitmap(32, 32);
            
            // Cria um objeto Graphics para desenhar no bitmap
            // Usa using para garantir liberação de recursos (Dispose Pattern)
            using (var g = Graphics.FromImage(bmp))
            {
                // Configura qualidade de renderização
                g.SmoothingMode = SmoothingMode.AntiAlias; // Suaviza bordas
                g.PixelOffsetMode = PixelOffsetMode.HighQuality; // Melhor precisão

                // Cria um pincel com a cor especificada
                // Usa using para garantir liberação de recursos
                using (var brush = new SolidBrush(color))
                {
                    // Switch pattern para diferentes tipos de ícone (Strategy Pattern)
                    switch (iconName.ToLower()) // Case-insensitive
                    {
                        case "play":
                            // Define pontos para um triângulo (ícone de play)
                            var playPoints = new PointF[] {
                                new PointF(10, 8),  // Ponto superior
                                new PointF(22, 16), // Ponto direito (ponta)
                                new PointF(10, 24)  // Ponto inferior
                            };
                            g.FillPolygon(brush, playPoints); // Preenche o triângulo
                            break;
                            
                        case "pause":
                            // Desenha dois retângulos verticais (ícone de pause)
                            g.FillRectangle(brush, 10, 8, 5, 16);  // Primeira barra
                            g.FillRectangle(brush, 17, 8, 5, 16); // Segunda barra
                            break;
                            
                        case "reset":
                            // Desenha um círculo com um recorte triangular
                            g.FillEllipse(brush, 8, 8, 16, 16); // Círculo base
                            
                            // Cria um recorte triangular no centro
                            using (var bgBrush = new SolidBrush(Color.Transparent))
                            {
                                g.FillPolygon(bgBrush, new PointF[] {
                                    new PointF(16, 12), // Topo do triângulo
                                    new PointF(20, 16), // Direita
                                    new PointF(16, 20)  // Base
                                });
                            }
                            break;
                            
                        case "menu":
                            // Desenha três barras horizontais (ícone de menu hamburguer)
                            g.FillRectangle(brush, 8, 10, 16, 3);  // Barra superior
                            g.FillRectangle(brush, 8, 15, 16, 3); // Barra do meio
                            g.FillRectangle(brush, 8, 20, 16, 3); // Barra inferior
                            break;
                            
                        default:
                            // Fallback: Desenha um ponto de interrogação para ícones desconhecidos
                            var font = new Font("Arial", 14, FontStyle.Bold);
                            g.DrawString("?", font, brush, new PointF(12, 8));
                            break;
                    }
                }
            }
            return bmp; // Retorna o ícone gerado
        }

        /// <summary>
        /// Clareia uma cor baseada em um fator (0-1)
        /// 
        /// Características:
        /// - Método puro: Não altera estado externo, apenas calcula novo valor
        /// - Funcional: Sem efeitos colaterais
        /// </summary>
        /// <param name="color">Cor original</param>
        /// <param name="factor">Fator de clareamento (0 = mesma cor, 1 = branco)</param>
        /// <returns>Nova cor clareada</returns>
        public static Color LightenColor(Color color, float factor)
        {
            // Fórmula para clarear cada componente RGB:
            // Novo valor = valor original + (255 - valor original) * fator
            return Color.FromArgb(
                (int)(color.R + (255 - color.R) * factor), // Vermelho
                (int)(color.G + (255 - color.G) * factor), // Verde
                (int)(color.B + (255 - color.B) * factor)  // Azul
            );
        }
    }
}