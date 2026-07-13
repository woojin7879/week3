# 🐻 곰 보스(Bear Boss) 전투 패턴 기획 및 추천 가이드

이 문서는 Unity 2D 플랫폼 게임의 씬 12 보스전 완성도를 극대화하기 위해 구현된 패턴 설명과 추가적으로 고려할 수 있는 보스 패턴 추천안을 담고 있습니다.

---

## 🎮 1. 현재 구현 완료된 보스 패턴 (Active Patterns)

플레이어와의 거리에 따라 대처하는 지능형 AI 루프가 탑재되어 있으며, 보스의 체력 상태에 따라 2페이즈 분노 모드가 발동합니다.

| 패턴명 | 사거리 | 작동 원리 및 밸런싱 핵심 |
| :--- | :--- | :--- |
| **추적 이동 (Chase)** | 전 범위 | 플레이어의 실시간 X좌표를 향해 방향을 flip하며 걸어갑니다. (1페이즈 속도: 1.5, 2페이즈 속도: 2.5) |
| **3연속 눈덩이 투척 (Triple Snowball Burst)** | 전 범위 (확률) | 청록색으로 몸을 번쩍이며 조준한 후, 0.25초 간격으로 눈덩이 3개를 연속 발사합니다. 투척 도중 플레이어가 넘어가면 실시간으로 고개를 돌려 조준합니다. |
| **점프 내려찍기 & 지진파 (Jump Slam & Shockwaves)** | 근거리 (확률) | 주황색으로 몸을 번쩍인 후 플레이어를 향해 높게 점프했다가 급강하하며 내려찍습니다. 안착 즉시 설정된 5개 플랫폼 Y축 높이 중 **무작위 2개 라인**을 타고 좌우로 느린 지진파 발사체를 내뿜습니다. |
| **2페이즈 분노 모드 (Rage Mode)** | 체력 50% 이하 (HP 1 이하) | 체력이 절반 이하가 되면 온몸이 붉게 변하며, **공격 쿨타임이 5초에서 3초로 대폭 단축**되고 돌진/이동 속도가 매우 빨라집니다. |

---

## 💡 2. 추가 추천 보스 패턴 기획안 (Recommended Patterns)

보스전을 한층 더 풍성하고 극적인 연출로 장식하기 위한 4가지 추가 패턴 추천입니다.

### 패턴 A: 🌀 포효 흡입 (Inhale Roar)
> **보스가 포효하며 중심부로 모든 중력을 집중시켜 다람쥐를 끌어당기는 패턴**

* **동작 설계**:
  1. 보스가 제자리에 멈춰 서서 흡입 연출(보라색 깜빡임 또는 이펙트)을 시작합니다.
  2. 2.0초 동안 플레이어의 Rigidbody2D에 보스 방향으로 지속적인 횡력(AddForce)을 가합니다.
  3. 플레이어는 끌려가지 않기 위해 반대 방향으로 계속 키를 누르며 저항하거나 기둥/발판 뒤로 피해야 합니다.
* **C# 구현 예시**:
  ```csharp
  IEnumerator InhaleRoutine() {
      isCharging = true;
      rigid.linearVelocity = Vector2.zero;
      
      float elapsed = 0f;
      while (elapsed < 2.0f) {
          // 끌어당김 경고 컬러링
          spriteRenderer.color = new Color(0.7f, 0.4f, 0.9f, 1f); 
          
          if (player != null) {
              Vector2 pullDir = (transform.position - player.position).normalized;
              Rigidbody2D playerRigid = player.GetComponent<Rigidbody2D>();
              if (playerRigid != null) {
                  // 플레이어를 곰 쪽으로 끌어당기는 힘 가함
                  playerRigid.AddForce(new Vector2(pullDir.x * 15f, 0), ForceMode2D.Force);
              }
          }
          elapsed += Time.deltaTime;
          yield return null;
      }
      spriteRenderer.color = Color.white;
      isCharging = false;
      Think();
  }
  ```

---

### 패턴 B: 🦔 롤링 스핀 대시 (Roll & Bounce)
> **보스가 둥글게 회전하여 벽을 튕기며 돌진하는 광폭화 돌격기**

* **동작 설계**:
  1. 보스가 몸을 웅크리는 회전 애니메이션을 재생하거나 스프라이트를 빠르게 자전시킵니다.
  2. 중력을 일시 무시(`gravityScale = 0`)하고 아주 빠른 속도로 좌/우 벽을 향해 굴러갑니다.
  3. 좌우 벽이나 타일맵 경계에 부딪힐 때마다 방향을 반대로 바꾸며 총 3회 왕복합니다.
  4. 다람쥐는 이 궤적을 피해 공중 발판 위로 점프하여 타이밍을 재야 합니다.
* **C# 구현 예시**:
  ```csharp
  IEnumerator RollBounceRoutine() {
      isCharging = true;
      float originalGravity = rigid.gravityScale;
      rigid.gravityScale = 0f;
      
      float rollSpeed = 22f;
      int bounceCount = 3;
      float direction = player.position.x - transform.position.x > 0 ? 1 : -1;
      
      while (bounceCount > 0) {
          rigid.linearVelocity = new Vector2(direction * rollSpeed, 0);
          
          // 벽 레이캐스트 감지
          Vector2 frontVec = new Vector2(rigid.position.x + direction * 1.0f, rigid.position.y);
          RaycastHit2D hit = Physics2D.Raycast(frontVec, new Vector2(direction, 0), 1.0f, LayerMask.GetMask("Platform"));
          if (hit.collider != null) {
              direction *= -1; // 반사
              bounceCount--;
          }
          yield return null;
      }
      
      rigid.gravityScale = originalGravity;
      isCharging = false;
      Think();
  }
  ```

---

### 패턴 C: 🧊 고드름 하강 배리어 (Icicle Rain Barrier)
> **하늘에서 무거운 얼음 고드름들이 수직 낙하해 바닥에 꽂혀 일시적인 물리 장벽을 형성하는 패턴**

* **동작 설계**:
  1. 보스가 발을 동동 구르면 화면 천장에서 날카로운 고드름(새로운 에셋/스프라이트) 3~4개가 플레이어 주변 바닥으로 떨어집니다.
  2. 고드름에 닿으면 플레이어는 데미지를 입습니다.
  3. 바닥에 꽂힌 고드름은 즉시 사라지지 않고 **2.5초간 물리 콜라이더를 지닌 기둥(장벽)**으로 유지됩니다.
  4. 이 장벽 때문에 플레이어와 보스 모두 이동이 제한되거나, 반대로 보스의 눈송이를 막아주는 엄폐물로 활용될 수 있는 전략적 패턴입니다.

---

### 패턴 D: 🌫️ 얼음 안개 순간이동 (Ice Mist Teleport)
> **보스가 얼음 안개 속에 숨어 사라진 뒤, 플레이어의 뒤쪽이나 공중 발판 위로 갑자기 나타나는 기습 패턴**

* **동작 설계**:
  1. 보스가 흐릿하게 투명해지며 알파 효과(Fade Out)와 함께 사라집니다. (콜라이더도 일시 OFF)
  2. 1초 뒤, 플레이어가 방심한 틈을 타 플레이어의 뒤쪽 4칸 지점 또는 2층 공중 발판 위에서 다시 선명하게 나타납니다(Fade In).
  3. 나타나자마자 바로 근접 할퀴기나 점프 공격을 전개하여 깜짝 긴장감을 연출합니다.
