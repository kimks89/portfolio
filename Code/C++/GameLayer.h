#ifndef __Game_Layer_H__
#define __Game_Layer_H__

#include "cocos2d.h"
#include "GameLayer.h"
#include "CatUnit.h"
#include "DogUnit.h"

using namespace cocos2d;

// ������
typedef enum
{
	kBackground,
	kMiddleground,
	kForeground,
	kpopup
};

// ���ֹ�ȣ
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

// ������, ����� �±�
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
	Size _screenSize; // ȭ��ũ��

	// �޼��� ����
	void createGameScreen(); // ȭ�� �ʱ�ȭ �޼���(ȭ���� ����)
	void createPools(); // ��ü Ǯ ���� �� �ʱ�ȭ �޼���
	void createAction(); // ���� �׼� ��ü ���� �޼���
	void createParticles(); // ��ƼŬ �ʱ�ȭ �޼���
	void setprogressbar();
	void setScore();
	void setMoney();
	// ĳ���� ���� �޼���
	void Dog();
	void Cat();
	void eraseEffect();

	// ��ƼŬ ����
	ParticleSystem* _snow;
	ParticleSystem* _snow2;
	ParticleSystem* _sun;
	ParticleSystem* _effect;

	//��ġ���
	//CCSpriteBatchNode* dog_UI;
	CCSpriteBatchNode* catAnimation;
	CCSpriteBatchNode* dogAnimation;
	CCSpriteBatchNode* ingameBatchNode;

	// ���
	Sprite* bg;
	// �̹��� ������ ������ �� ĳ����
	Sprite* uiIcon1;
	Sprite* uiIcon2;
	Sprite* uiIcon3;
	// ��(����)
	Sprite* catHouse; // ����� ��
	Sprite* dogHouse; // �� ��
	// UI
	Sprite* scoreIcon;
	Sprite* moneyIcon;
	Sprite* unitMask1;
	Sprite* unitMask2;
	Sprite* unitMask3;

	// ĳ���� Ǯ �̹���
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
	// ����� �浹
	Rect catsHouse;

	Rect dogsHouse;

	/////////////////////////////////
	// Ǯ �̹��� (����)
	Vector<DogUnit*> _dogPool; // ������1 Ǯ
	
	int _dogPoolIndex; // ������ ����� ������ ��ġ
	int _dogPoolIndex2;
	int _dogPoolIndex3;

	Vector<CatUnit*> _catPool; // ����� Ǯ
	
	int _catPoolIndex; //������ ����� ����� ��ġ
	int _catPoolIndex2;
	int _catPoolIndex3;

	float _catInterval; // ���� ���� ����1
	float _catTimer; // ���� ���� �� ��� �ð�

	float _catInterval2; // ���� ���� ����2
	float _catTimer2; // ���� ���� �� ��� �ð�

	float _catInterval3; // ���� ���� ����3
	float _catTimer3; // ���� ���� �� ��� �ð�

	Vector<DogUnit*> _dog01Objects; // ȭ��󿡼� �������� ��ü �÷���
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

	//���ھ�
	int score = 0;
	CCLabelBMFont* scoreFont;

	// ��
	int money = 0;
	CCLabelBMFont* moneyFont;
	float moneyTime = 0;
	float moneyRisenTime = 0.01f;

	// �ð�
	int time = 0;

	//GameOver
	Sprite* _gameClear;
	Sprite* _gameOver;

	//�ִϸ��̼�
	Action* catmove;
	Action* dogmove;

	Action* effectAnimation;//�浹�ִϸ��̼�

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
