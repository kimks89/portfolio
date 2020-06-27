#ifndef __Game_Layer_H__
#define __Game_Layer_H__

#include "cocos2d.h"
#include "GameLayer.h"
#include "CatUnit.h"
#include "DogUnit.h"

using namespace cocos2d;

// 배경순서
typedef enum
{
	kBackground,
	kMiddleground,
	kForeground,
	kpopup
};

// 유닛번호
typedef enum
{
	unit1,
	unit2,
	unit3,
	cat1,
	cat2,
	cat3,
	pauseButton_tag
};

// 강아지, 고양이 태그
typedef enum
{
	dogtag01,
	dogtag02,
	dogtag03,
	cattag01,
	cattag02,
	cattag03
};

class GameLayer : public cocos2d::Layer
{
private:
	Size _screenSize; // 화면크기

	// 메서드 관리
	void createGameScreen(); // 화면 초기화 메서드(화면을 셋팅)
	void createPools(); // 객체 풀 생성 및 초기화 메서드
	void createAction(); // 재사용 액션 객체 생성 메서드
	void createParticles(); // 파티클 초기화 메서드
	void setprogressbar();
	void setScore();
	void setMoney();
	// 캐릭터 생성 메서드
	void Dog();
	void Cat();
	void eraseEffect();

	// 파티클 관리
	ParticleSystem* _snow;
	ParticleSystem* _snow2;
	ParticleSystem* _sun;
	ParticleSystem* _effect;

	//배치노드
	//CCSpriteBatchNode* dog_UI;
	CCSpriteBatchNode* catAnimation;
	CCSpriteBatchNode* dogAnimation;
	CCSpriteBatchNode* ingameBatchNode;

	// 배경
	Sprite* bg;
	// 이미지 강아지 아이콘 및 캐릭터
	Sprite* uiIcon1;
	Sprite* uiIcon2;
	Sprite* uiIcon3;
	// 집(기지)
	Sprite* catHouse; // 고양이 집
	Sprite* dogHouse; // 개 집
	// UI
	Sprite* scoreIcon;
	Sprite* moneyIcon;
	Sprite* unitMask1;
	Sprite* unitMask2;
	Sprite* unitMask3;

	// 캐릭터 풀 이미지
	CatUnit* cat01All;
	CatUnit* cat02All;
	CatUnit* cat03All;
	DogUnit* dog01All;
	DogUnit* dog02All;
	DogUnit* dog03All;

	Sprite* effectImg;
	Sprite* playerEmptyHP;
	Sprite* amrmyEmptyHP;
	Sprite* playerFullHPbar;
	Sprite* playerFront;
	Sprite* amrmyFullHPbar;
	Sprite* amrmyFront;

	//pause menu
	bool _gamePause = false;

	Sprite* _popupwindow;
	Sprite* _pauseText;
	Sprite* _backMenuButton;
	Sprite* _backMenuButtonOn;
	Sprite* _continuButton;
	Sprite* _continuButtonOn;
	Sprite* _exitButton;
	Sprite* _exitButtonOn;

	Menu* _pauseMenu;


	bool isPointln(Node* sender, const Point& worldPoint);
	// 고양이 충돌
	Rect catsHouse;

	Rect dogsHouse;

	/////////////////////////////////
	// 풀 이미지 (수정)
	Vector<DogUnit*> _dogPool; // 강아지1 풀
	
	int _dogPoolIndex; // 다음에 사용할 강아지 위치
	int _dogPoolIndex2;
	int _dogPoolIndex3;

	Vector<CatUnit*> _catPool; // 고양이 풀
	
	int _catPoolIndex; //다음에 사용할 고양이 위치
	int _catPoolIndex2;
	int _catPoolIndex3;

	float _catInterval; // 냥이 생성 간격1
	float _catTimer; // 냥이 생성 후 경과 시간

	float _catInterval2; // 냥이 생성 간격2
	float _catTimer2; // 냥이 생성 후 경과 시간

	float _catInterval3; // 냥이 생성 간격3
	float _catTimer3; // 냥이 생성 후 경과 시간

	Vector<DogUnit*> _dog01Objects; // 화면상에서 동작중인 객체 컬렉션
	Vector<DogUnit*> _dog02Objects;
	Vector<DogUnit*> _dog03Objects;

	Vector<CatUnit*> _cat01Objects;
	Vector<CatUnit*> _cat02Objects;
	Vector<CatUnit*> _cat03Objects;

	//////////// UI ////////////////////
	//HP
	int playerfullHP = 3000;
	int amrmyfullHP = 3000;
	int playerHP = 3000;
	int amrmyHP = 3000;

	int dog01Hp = 500;
	int dog02Hp = 700;
	int dog03Hp = 1000;
	int cat01Hp = 500;
	int cat02Hp = 700;
	int cat03Hp = 1000;

	//Pool
	const int poolMax = 50;

	//스코어
	int score = 0;
	CCLabelBMFont* scoreFont;

	// 돈
	int money = 0;
	CCLabelBMFont* moneyFont;
	float moneyTime = 0;
	float moneyRisenTime = 0.01f;

	// 시간
	int time = 0;

	//GameOver
	Sprite* _gameClear;
	Sprite* _gameOver;

	//애니메이션
	Action* catmove;
	Action* dogmove;

	Action* effectAnimation;//충돌애니메이션

	int WALKING = 0;
	int ATTACKING = 2;

	CatUnit* catunit;
	DogUnit* dogunit;

	bool gameover = false;
	bool gameclear = false;

public:
	static cocos2d::Scene* createScene();
	virtual bool init();
	void update(float dt);
	void _pauseMenuSeletion(Ref* sender, int num);
	void _gameOvers();
	void _newScene(float dt);
	virtual void onTouchesBegan(const std::vector<Touch*>& touches, Event* event);
	virtual void onTouchesMoved(const std::vector<Touch*>& touches, Event* event);
};
#endif 
