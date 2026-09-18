import { PlayerData } from '../../player/PlayerModel';
import { TournamentInstance } from '../../tournament/TournamentModel';
import { CareerStatisticsContainer } from '../../player/CareerStatistics';
import { CAREER_LEVEL_ORDER, CAREER_LEVEL_CONFIG } from '../../career/CareerLevel';
import { CareerProgression } from '../../career/CareerProgression';
import { Formatters } from '../../utilities/Formatters';
import { TournamentManager } from '../../tournament/TournamentManager';
import { GameStateManager, GameScreen } from '../../core/GameState';
import { EventBus } from '../../core/EventBus';
import { AudioManager } from '../../audio/AudioManager';
import { FatigueState } from '../../career/FatigueSystem';
import { ActiveInjury } from '../../career/InjurySystem';
import { WeeklySchedule, CalendarDay } from '../../career/CareerCalendar';
import { TrainingCategory } from '../../career/TrainingSystem';

export class CareerHubScreen {
  public static render(
    player: PlayerData,
    tournament: TournamentInstance | null,
    stats: CareerStatisticsContainer,
    fatigue?: FatigueState,
    injury?: ActiveInjury | null,
    calendar?: WeeklySchedule,
    unlockedCount: number = 0
  ): string {
    const nextFixture = tournament ? TournamentManager.getNextFixture(tournament) : null;
    const currentMeta = CAREER_LEVEL_CONFIG[player.career.currentCareerLevel];
    const levelStats = stats.byLevel[player.career.currentCareerLevel] || {
      batting: stats.allTime.batting,
      bowling: stats.allTime.bowling,
      fielding: stats.allTime.fielding
    };

    const promoCheck = CareerProgression.checkPromotion(
      player,
      levelStats.batting.matches,
      levelStats.batting.runs,
      levelStats.bowling.wickets
    );

    const fatigueVal = fatigue ? fatigue.fatigueLevel : 10;
    const currentDay = calendar?.slots[calendar.currentDayIndex]?.day || CalendarDay.MONDAY;
    const currentActivity = calendar?.slots[calendar.currentDayIndex]?.activity || 'TRAINING';

    // Career ladder HTML
    const ladderHtml = CAREER_LEVEL_ORDER.map((lvl, index) => {
      const isCurrent = lvl === player.career.currentCareerLevel;
      const currentIndex = CAREER_LEVEL_ORDER.indexOf(player.career.currentCareerLevel);
      const isCompleted = index < currentIndex;
      const meta = CAREER_LEVEL_CONFIG[lvl];

      let stateClass = 'locked';
      if (isCurrent) stateClass = 'current';
      else if (isCompleted) stateClass = 'completed';

      return `
        <div class="ladder-node ${stateClass}">
          <div class="ladder-node-circle">
            ${isCompleted ? '✓' : index + 1}
          </div>
          <div class="ladder-node-label">${meta.displayName.split(' ')[0]}</div>
        </div>
      `;
    }).join('');

    return `
      <div class="main-viewport">
        <!-- Hero Banner: Player Profile & Quick Readiness -->
        <div class="hero-banner">
          <div>
            <div style="display: flex; align-items: center; gap: 0.75rem; margin-bottom: 0.5rem;">
              <span class="badge badge-gold">${player.role.toUpperCase()}</span>
              <span class="badge ${Formatters.getFormBadgeClass(player.mental.form)}">STATUS: ${player.career.careerStatus}</span>
              ${injury ? `<span class="badge badge-danger">INJURED: ${injury.name} (${injury.daysRemaining}d)</span>` : ''}
              <span style="font-size: 0.85rem; color: var(--text-dim);">#${player.jerseyNumber} • ${player.nationality}</span>
            </div>

            <h1 style="font-family: var(--font-display); font-size: 3.5rem; line-height: 1; letter-spacing: 0.05em; font-weight: 700;">
              ${player.firstName.toUpperCase()} <span style="color: var(--accent-emerald);">${player.lastName.toUpperCase()}</span>
            </h1>

            <div style="display: flex; gap: 1.5rem; margin-top: 0.85rem; font-size: 0.95rem;">
              <div><strong style="color: var(--text-dim);">Age:</strong> ${player.age}</div>
              <div><strong style="color: var(--text-dim);">Current Team:</strong> ${player.career.currentTeam}</div>
              <div><strong style="color: var(--text-dim);">Level:</strong> <span style="color: var(--accent-cyan); font-weight: 600;">${currentMeta.displayName}</span></div>
            </div>
          </div>

          <div style="text-align: right; z-index: 2;">
            <div style="font-family: var(--font-display); font-size: 4.5rem; line-height: 1; color: var(--accent-gold); font-weight: 700;">
              ${player.potential.overallRating}
            </div>
            <div style="font-size: 0.75rem; color: var(--text-dim); text-transform: uppercase; letter-spacing: 0.1em; font-weight: 600;">
              Overall Rating • POT ${player.potential.potentialRating}
            </div>
            ${promoCheck.canPromote ? `
              <button id="btn-claim-promotion" class="btn btn-gold" style="margin-top: 1rem; animation: pulse 2s infinite;">
                🌟 PROMOTION READY!
              </button>
            ` : ''}
          </div>
        </div>

        <!-- Career Progression Visual Roadmap -->
        <div class="card" style="margin-bottom: 2rem;">
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem;">
            <strong style="font-size: 0.95rem; letter-spacing: 0.05em; text-transform: uppercase; color: var(--text-muted);">
              Career Tier Road Map
            </strong>
            <button id="btn-view-roadmap" class="btn btn-secondary" style="padding: 0.35rem 0.75rem; font-size: 0.8rem;">
              View Requirements & Details →
            </button>
          </div>
          <div class="career-ladder">
            ${ladderHtml}
          </div>
        </div>

        <!-- Quick Calendar & Milestones Bar -->
        <div class="grid-2" style="margin-bottom: 1.5rem;">
          <div class="card" style="display: flex; justify-content: space-between; align-items: center;">
            <div>
              <div style="font-size: 0.75rem; color: var(--text-dim); font-weight: 700;">WEEKLY CALENDAR</div>
              <div style="font-size: 1.05rem; font-weight: 700; color: #fff;">
                ${currentDay}: <span style="color: var(--accent-gold);">${currentActivity}</span>
              </div>
            </div>
            <button id="btn-quick-calendar" class="btn btn-secondary" style="font-size: 0.8rem; padding: 0.4rem 0.85rem;">
              View Schedule →
            </button>
          </div>

          <div class="card" style="display: flex; justify-content: space-between; align-items: center;">
            <div>
              <div style="font-size: 0.75rem; color: var(--text-dim); font-weight: 700;">TROPHIES & ACHIEVEMENTS</div>
              <div style="font-size: 1.05rem; font-weight: 700; color: var(--accent-gold);">
                🏆 ${unlockedCount} / 16 Milestones Unlocked
              </div>
            </div>
            <button id="btn-quick-milestones" class="btn btn-secondary" style="font-size: 0.8rem; padding: 0.4rem 0.85rem;">
              Trophy Cabinet →
            </button>
          </div>
        </div>

        <!-- 3-Column Dashboard -->
        <div class="grid-3">
          <!-- Readiness / Mental Meters -->
          <div class="card">
            <h3 style="font-size: 1rem; margin-bottom: 1.25rem; font-weight: 700; color: #fff; border-bottom: 1px solid var(--border-color); padding-bottom: 0.5rem;">
              CONDITION, FORM & FATIGUE
            </h3>

            <div style="display: flex; flex-direction: column; gap: 1.15rem;">
              <div>
                <div style="display: flex; justify-content: space-between; font-size: 0.85rem;">
                  <span>Form</span>
                  <strong style="color: ${player.mental.form >= 60 ? 'var(--accent-emerald)' : '#f87171'};">${player.mental.form} / 100</strong>
                </div>
                <div class="stat-meter">
                  <div class="stat-meter-fill" style="width: ${player.mental.form}%; background: ${player.mental.form >= 60 ? 'var(--accent-emerald)' : '#ef4444'};"></div>
                </div>
              </div>

              <div>
                <div style="display: flex; justify-content: space-between; font-size: 0.85rem;">
                  <span>Confidence</span>
                  <strong style="color: var(--accent-gold);">${player.mental.confidence} / 100</strong>
                </div>
                <div class="stat-meter">
                  <div class="stat-meter-fill" style="width: ${player.mental.confidence}%; background: var(--accent-gold);"></div>
                </div>
              </div>

              <div>
                <div style="display: flex; justify-content: space-between; font-size: 0.85rem;">
                  <span>Fitness & Stamina</span>
                  <strong style="color: var(--accent-cyan);">${player.mental.fitness}%</strong>
                </div>
                <div class="stat-meter">
                  <div class="stat-meter-fill" style="width: ${player.mental.fitness}%; background: var(--accent-cyan);"></div>
                </div>
              </div>

              <div>
                <div style="display: flex; justify-content: space-between; font-size: 0.85rem;">
                  <span>Workload Fatigue</span>
                  <strong style="color: ${fatigueVal > 65 ? '#ef4444' : (fatigueVal > 40 ? 'var(--accent-gold)' : 'var(--accent-emerald)')};">
                    ${fatigueVal} / 100
                  </strong>
                </div>
                <div class="stat-meter">
                  <div class="stat-meter-fill" style="width: ${fatigueVal}%; background: ${fatigueVal > 65 ? '#ef4444' : (fatigueVal > 40 ? 'var(--accent-gold)' : 'var(--accent-emerald)')};"></div>
                </div>
              </div>
            </div>

            <!-- Training & Physio Dropdown Actions -->
            <div style="margin-top: 1.5rem; display: flex; flex-direction: column; gap: 0.5rem;">
              <div style="display: flex; gap: 0.5rem;">
                <select id="select-training-drill" style="flex: 1; padding: 0.4rem; background: var(--bg-main); border: 1px solid var(--border-color); border-radius: 6px; color: #fff; font-size: 0.8rem;">
                  <option value="${TrainingCategory.BATTING_TECHNIQUE}">${TrainingCategory.BATTING_TECHNIQUE}</option>
                  <option value="${TrainingCategory.BATTING_POWER}">${TrainingCategory.BATTING_POWER}</option>
                  <option value="${TrainingCategory.BOWLING_ACCURACY}">${TrainingCategory.BOWLING_ACCURACY}</option>
                  <option value="${TrainingCategory.BOWLING_VARIATIONS}">${TrainingCategory.BOWLING_VARIATIONS}</option>
                  <option value="${TrainingCategory.FIELDING_REFLEXES}">${TrainingCategory.FIELDING_REFLEXES}</option>
                  <option value="${TrainingCategory.FITNESS_STAMINA}">${TrainingCategory.FITNESS_STAMINA}</option>
                  <option value="${TrainingCategory.MENTAL_COMPOSURE}">${TrainingCategory.MENTAL_COMPOSURE}</option>
                </select>
                <button id="btn-execute-drill" class="btn btn-secondary" style="font-size: 0.8rem; padding: 0.4rem 0.75rem;">
                  Train 🏋️
                </button>
              </div>

              <button id="btn-physio-rest" class="btn btn-secondary" style="width: 100%; font-size: 0.8rem; padding: 0.4rem;">
                🧘 Physio & Rest Recovery (-20 Fatigue)
              </button>
            </div>
          </div>

          <!-- Active Tournament & Next Fixture -->
          <div class="card" style="display: flex; flex-direction: column; justify-content: space-between;">
            <div>
              <div style="display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 0.75rem;">
                <h3 style="font-size: 1rem; font-weight: 700; color: #fff;">
                  ACTIVE TOURNAMENT
                </h3>
                <span class="badge badge-primary">${tournament?.type || 'CUP'}</span>
              </div>

              <div style="background: var(--bg-card-alt); border-radius: 8px; padding: 1rem; border: 1px solid var(--border-color); margin-bottom: 1rem;">
                <div style="font-weight: 700; font-size: 1.1rem; color: #fff; margin-bottom: 0.25rem;">
                  ${tournament?.name || 'Under-16 State Youth Cup'}
                </div>
                <div style="font-size: 0.8rem; color: var(--text-dim);">
                  Format: ${tournament?.format} • Round ${tournament?.currentRound} of ${tournament?.totalMatches}
                </div>
              </div>

              ${nextFixture ? `
                <div style="font-size: 0.85rem; margin-bottom: 0.5rem; font-weight: 600; color: var(--text-muted);">
                  NEXT SCHEDULED MATCH:
                </div>
                <div style="display: flex; align-items: center; justify-content: space-between; background: var(--bg-main); padding: 0.85rem; border-radius: 8px; border: 1px solid var(--border-color);">
                  <div style="font-weight: 700; font-size: 0.95rem;">${nextFixture.teamAName}</div>
                  <div style="color: var(--accent-gold); font-size: 0.75rem; font-weight: 800;">VS</div>
                  <div style="font-weight: 700; font-size: 0.95rem;">${nextFixture.teamBName}</div>
                </div>
                <div style="font-size: 0.75rem; color: var(--text-dim); margin-top: 0.4rem; text-align: center;">
                  📍 ${nextFixture.venueName}
                </div>
              ` : `
                <div style="padding: 1rem; text-align: center; color: var(--accent-gold);">
                  🏆 Tournament Completed! All fixtures concluded.
                </div>
              `}
            </div>

            <div style="margin-top: 1.5rem; display: flex; gap: 0.75rem;">
              <button id="btn-view-tournament" class="btn btn-secondary" style="flex: 1; font-size: 0.85rem;">
                Standings Table
              </button>
              ${nextFixture ? `
                <button id="btn-play-match" class="btn btn-primary" style="flex: 1.2; font-size: 0.85rem;">
                  🏏 Play Match →
                </button>
              ` : `
                <button id="btn-new-tournament" class="btn btn-gold" style="flex: 1.2; font-size: 0.85rem;">
                  🔄 Next Tournament
                </button>
              `}
            </div>
          </div>

          <!-- Career Stats Snapshot & Objectives -->
          <div class="card" style="display: flex; flex-direction: column; justify-content: space-between;">
            <div>
              <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; border-bottom: 1px solid var(--border-color); padding-bottom: 0.5rem;">
                <h3 style="font-size: 1rem; font-weight: 700; color: #fff;">
                  CAREER SUMMARY
                </h3>
                <button id="btn-full-stats" class="btn btn-secondary" style="padding: 0.25rem 0.6rem; font-size: 0.75rem;">
                  Full Stats
                </button>
              </div>

              <div class="grid-2" style="gap: 0.75rem; margin-bottom: 1.25rem;">
                <div style="background: var(--bg-card-alt); padding: 0.75rem; border-radius: 8px;">
                  <div style="font-size: 0.75rem; color: var(--text-dim);">MATCHES</div>
                  <div style="font-size: 1.35rem; font-weight: 700;">${stats.allTime.batting.matches}</div>
                </div>
                <div style="background: var(--bg-card-alt); padding: 0.75rem; border-radius: 8px;">
                  <div style="font-size: 0.75rem; color: var(--text-dim);">RUNS</div>
                  <div style="font-size: 1.35rem; font-weight: 700; color: var(--accent-emerald);">${stats.allTime.batting.runs}</div>
                </div>
                <div style="background: var(--bg-card-alt); padding: 0.75rem; border-radius: 8px;">
                  <div style="font-size: 0.75rem; color: var(--text-dim);">BATTING AVG</div>
                  <div style="font-size: 1.35rem; font-weight: 700;">${Formatters.formatAverage(stats.allTime.batting.average)}</div>
                </div>
                <div style="background: var(--bg-card-alt); padding: 0.75rem; border-radius: 8px;">
                  <div style="font-size: 0.75rem; color: var(--text-dim);">WICKETS</div>
                  <div style="font-size: 1.35rem; font-weight: 700; color: var(--accent-cyan);">${stats.allTime.bowling.wickets}</div>
                </div>
              </div>

              <div style="background: rgba(56, 189, 248, 0.05); border: 1px solid rgba(56, 189, 248, 0.2); border-radius: 8px; padding: 0.85rem;">
                <strong style="font-size: 0.8rem; color: var(--accent-cyan); display: block; margin-bottom: 0.25rem;">
                  🎯 CURRENT PROMOTION GOAL:
                </strong>
                <div style="font-size: 0.8rem; color: var(--text-muted);">
                  ${promoCheck.missingRequirements[0] || 'All criteria met! Keep performing.'}
                </div>
              </div>
            </div>

            <div style="margin-top: 1.25rem;">
              <button id="btn-save-career-now" class="btn btn-secondary" style="width: 100%; font-size: 0.85rem;">
                💾 Save Career Progress
              </button>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  public static bindEvents(): void {
    const playBtn = document.getElementById('btn-play-match');
    if (playBtn) {
      playBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        GameStateManager.getInstance().transitionTo(GameScreen.MATCH_PREPARATION);
      });
    }

    const claimPromoBtn = document.getElementById('btn-claim-promotion');
    if (claimPromoBtn) {
      claimPromoBtn.addEventListener('click', () => {
        AudioManager.getInstance().playFanfare();
        EventBus.getInstance().emit('career:claim-promotion');
      });
    }

    const roadmapBtn = document.getElementById('btn-view-roadmap');
    if (roadmapBtn) {
      roadmapBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        GameStateManager.getInstance().transitionTo(GameScreen.CAREER_PROGRESSION);
      });
    }

    const tournBtn = document.getElementById('btn-view-tournament');
    if (tournBtn) {
      tournBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        GameStateManager.getInstance().transitionTo(GameScreen.TOURNAMENT);
      });
    }

    const newTournBtn = document.getElementById('btn-new-tournament');
    if (newTournBtn) {
      newTournBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        EventBus.getInstance().emit('tournament:start-new');
      });
    }

    const statsBtn = document.getElementById('btn-full-stats');
    if (statsBtn) {
      statsBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        GameStateManager.getInstance().transitionTo(GameScreen.STATISTICS);
      });
    }

    const saveBtn = document.getElementById('btn-save-career-now');
    if (saveBtn) {
      saveBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        EventBus.getInstance().emit('game:manual-save');
      });
    }

    const calBtn = document.getElementById('btn-quick-calendar');
    if (calBtn) {
      calBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        GameStateManager.getInstance().transitionTo(GameScreen.CALENDAR);
      });
    }

    const milBtn = document.getElementById('btn-quick-milestones');
    if (milBtn) {
      milBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        GameStateManager.getInstance().transitionTo(GameScreen.MILESTONES);
      });
    }

    const drillBtn = document.getElementById('btn-execute-drill');
    if (drillBtn) {
      drillBtn.addEventListener('click', () => {
        AudioManager.getInstance().playBatHit();
        const sel = (document.getElementById('select-training-drill') as HTMLSelectElement)?.value as TrainingCategory;
        EventBus.getInstance().emit('player:drill-train', sel);
      });
    }

    const physioBtn = document.getElementById('btn-physio-rest');
    if (physioBtn) {
      physioBtn.addEventListener('click', () => {
        AudioManager.getInstance().playClick();
        EventBus.getInstance().emit('player:physio-recovery');
      });
    }
  }
}
