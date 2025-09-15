using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace CronometroRegressivo.UI.Controls
{
    /// <summary>
    /// Painel personalizado que permite exibir imagens com controle de opacidade
    /// 
    /// Padrões e conceitos utilizados:
    /// - Herança: Estende a classe Panel padrão do Windows Forms
    /// - Custom Painting: Sobrescreve o método OnPaint para renderização personalizada
    /// - Encapsulamento: Expõe apenas a propriedade Opacity necessária
    /// - Gerenciamento de Recursos: Uso correto de objetos IDisposable
    /// </summary>
    public class ImagePanel : Panel
    {
        /// <summary>
        /// Nível de opacidade da imagem (0.0 a 1.0)
        /// - Valor padrão: 0.25f (25% de opacidade)
        /// - 0.0 = Totalmente transparente
        /// - 1.0 = Totalmente opaco
        /// </summary>
        public float Opacity { get; set; } = 0.25f;

        /// <summary>
        /// Método sobrescrito para renderização personalizada do painel
        /// 
        /// Técnicas utilizadas:
        /// - ColorMatrix: Para controle avançado de opacidade
        /// - ImageAttributes: Para aplicar transformações na imagem
        /// - Double Buffering: Herdado da classe Panel para reduzir flickering
        /// </summary>
        /// <param name="e">Argumentos de pintura que contêm o objeto Graphics</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Chama a implementação base primeiro
            base.OnPaint(e);

            // Verifica se há uma imagem de fundo definida
            if (BackgroundImage != null)
            {
                // Cria uma matriz de cores para controlar a opacidade
                // Matrix33 controla o componente alfa (transparência)
                var colorMatrix = new ColorMatrix { Matrix33 = Opacity };

                // Cria atributos de imagem para aplicar a transformação
                // Usa using para garantir liberação de recursos (Dispose Pattern)
                using (var attributes = new ImageAttributes())
                {
                    // Aplica a matriz de cores aos atributos da imagem
                    attributes.SetColorMatrix(colorMatrix);

                    // Desenha a imagem com os atributos de transparência
                    e.Graphics.DrawImage(
                        BackgroundImage,                          // Imagem original
                        new Rectangle(0, 0, Width, Height),       // Retângulo de destino
                        0, 0,                                    // Coordenadas de origem (X,Y)
                        BackgroundImage.Width,                    // Largura original
                        BackgroundImage.Height,                   // Altura original
                        GraphicsUnit.Pixel,                       // Unidade de medida
                        attributes                                // Atributos com opacidade
                    );
                }
            }
        }
    }
}