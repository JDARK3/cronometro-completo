using System;
using System.Collections.Generic;
using System.Linq;
using CronometroRegressivo.Core.Models;

namespace CronometroRegressivo.Core.Managers
{
    /// <summary>
    /// Classe estática responsável por gerenciar todas as operações CRUD
    /// relacionadas aos temporizadores.
    /// Implementa o padrão Repository para operações de persistência.
    /// </summary>
    public static class TimerManager
    {
        // Lista em memória que simula um banco de dados (para fins de exemplo)
        private static readonly List<TimerModel> _timers = new List<TimerModel>();

        // Contador para geração de IDs (simulando auto-incremento)
        private static int _lastId = 0;

        /// <summary>
        /// Construtor estático que inicializa dados de exemplo
        /// </summary>
        static TimerManager()
        {
            // CREATE - Adiciona temporizadores de exemplo
            SaveTimer(new TimerModel { 
                Name = "Intervalo Curto", 
                TimeInSeconds = 300,  // 5 minutos
                HasSoundNotification = true,
                ShouldRepeat = false,
                InitialSeconds = 300
            });
            
            SaveTimer(new TimerModel { 
                Name = "Intervalo Longo", 
                TimeInSeconds = 900,  // 15 minutos
                HasSoundNotification = false,
                ShouldRepeat = false,
                InitialSeconds = 900
            });
        }

        /// <summary>
        /// Salva ou atualiza um temporizador (CREATE/UPDATE do CRUD)
        /// </summary>
        /// <param name="timer">Objeto TimerModel a ser salvo</param>
        /// <exception cref="ArgumentNullException">Se o temporizador for nulo</exception>
        /// <exception cref="ArgumentException">Se nome for vazio ou tempo inválido</exception>
        public static void SaveTimer(TimerModel timer)
        {
            // Validação de entrada
            if (timer == null)
                throw new ArgumentNullException(nameof(timer));

            if (string.IsNullOrWhiteSpace(timer.Name))
                throw new ArgumentException("Nome do temporizador não pode ser vazio");

            if (timer.TimeInSeconds <= 0)
                throw new ArgumentException("Duração deve ser maior que zero");

            // Se é um novo timer, define o tempo inicial
            if (timer.Id == 0)
            {
                timer.InitialSeconds = timer.TimeInSeconds;
            }

            // UPDATE - Se o temporizador já existe (tem ID)
            if (timer.Id > 0)
            {
                var existingTimer = _timers.FirstOrDefault(t => t.Id == timer.Id);
                if (existingTimer != null)
                {
                    // Atualiza todas as propriedades
                    existingTimer.Name = timer.Name;
                    existingTimer.TimeInSeconds = timer.TimeInSeconds;
                    existingTimer.HasSoundNotification = timer.HasSoundNotification;
                    existingTimer.ShouldRepeat = timer.ShouldRepeat;
                    
                    // ATUALIZA O TEMPO INICIAL para o novo valor
                    // Isso garante que o loop use o tempo editado, não o original
                    existingTimer.InitialSeconds = timer.TimeInSeconds;
                }
            }
            else // CREATE - Novo temporizador
            {
                timer.Id = ++_lastId; // Gera novo ID
                _timers.Add(timer);  // Adiciona na lista
            }
        }

        /// <summary>
        /// Obtém todos os temporizadores ordenados por nome (READ do CRUD)
        /// </summary>
        /// <returns>Lista de temporizadores</returns>
        public static List<TimerModel> GetTimers()
        {
            return _timers.OrderBy(t => t.Name).ToList();
        }

        /// <summary>
        /// Obtém um temporizador específico por ID (READ do CRUD)
        /// </summary>
        /// <param name="id">ID do temporizador</param>
        /// <returns>TimerModel ou null se não encontrado</returns>
        public static TimerModel? GetTimer(int id)
        {
            return _timers.FirstOrDefault(t => t.Id == id);
        }

        /// <summary>
        /// Remove um temporizador (DELETE do CRUD)
        /// </summary>
        /// <param name="id">ID do temporizador a ser removido</param>
        /// <returns>True se removido com sucesso</returns>
        public static bool DeleteTimer(int id)
        {
            var timerToRemove = _timers.FirstOrDefault(t => t.Id == id);
            if (timerToRemove != null)
            {
                return _timers.Remove(timerToRemove);
            }
            return false;
        }
    }
}