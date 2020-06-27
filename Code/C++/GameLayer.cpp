#include "GameLayer.h"
#include "MainLayer.h"
#include "CatUnit.h"
#include "DogUnit.h"

USING_NS_CC;

using namespace CocosDenshion;

Scene* GameLayer::createScene()
{

	auto scene = Scene::create();
	auto layer = GameLayer::create();
	scene->addChild(layer);
	return scene;
}

bool GameLayer::init()
{
	if (Layer::init() == false)
	{
		return false;
	}
	auto bglayer = Layer::create();
	_screenSize = Director::getInstance()->getWinSize();

	createGameScreen();
	createParticles();
	createPools();
	resetGame();

	// ��� ���� ���
	SimpleAudioEngine::getInstance()->playBackgroundMusic("ingamebgm.mp3", true);

	//�̺�Ʈ ó���� ���� �� �޼��� ����
	EventListenerTouchAllAtOnce* _touchListener = EventListenerTouchAllAtOnce::create();
	_touchListener->onTouchesBegan = CC_CALLBACK_2(GameLayer::onTouchesBegan, this);
	_touchListener->onTouchesMoved = CC_CALLBACK_2(GameLayer::onTouchesMoved, this);
	_touchListener->onTouchesEnded = CC_CALLBACK_2(GameLayer::onTouchesEnded, this);

	getEventDispatcher()->addEventListenerWithSceneGraphPriority(_touchListener, this);

	scheduleUpdate();

	return true;
}

// �̹��� ���
void GameLayer::createGameScreen()
{

	SpriteFrameCache* cache = SpriteFrameCache::getInstance();

	cache->addSpriteFramesWithFile("mainui.plist");

	cache->addSpriteFramesWithFile("doganimation.plist");
	dogAnimation = SpriteBatchNode::create("doganimation.png");
	addChild(dogAnimation);

	cache->addSpriteFramesWithFile("ingameui.plist");
	ingameBatchNode = SpriteBatchNode::create("ingameui.png");
	addChild(ingameBatchNode);

	cache->addSpriteFramesWithFile("catanimation.plist");
	catAnimation = SpriteBatchNode::create("catanimation.png");
	addChild(catAnimation);


	//////////////////////////////////////////////////////////////////////////////////////////
	//���
	bg = Sprite::create("bg.png"); // ��� ���
	bg->setPosition(_screenSize.width * 960 / 1280, _screenSize.height * 410 / 720);
	bg->setScaleY(1.05f);
	this->addChild(bg, kBackground);

	//����
	dogHouse = Sprite::createWithSpriteFrameName("dogshouse.png");
	dogHouse->setPosition(dogHouse->getContentSize().width * 0.5f, _screenSize.height * 180 / 720);
	dogHouse->setScaleY(0.8f);
	bg->addChild(dogHouse, kBackground);

	//�������
	catHouse = Sprite::createWithSpriteFrameName("catshouse.png");
	catHouse->setPosition(bg->getContentSize().width - (catHouse->getContentSize().width * 0.5f), _screenSize.height * 180 / 720);
	catHouse->setScaleY(0.8f);
	bg->addChild(catHouse, kBackground);
	//////////////////////////////////////////////////////////////////////////////////////////
	//UI��
	Sprite* uiPanel = Sprite::createWithSpriteFrameName("dogselectui.png");
	uiPanel->setPosition(_screenSize.width * 0.5f, _screenSize.height * 58 / 720);
	uiPanel->setScaleY(0.8f);
	this->addChild(uiPanel, kForeground);

	uiIcon1 = Sprite::createWithSpriteFrameName("unit_icon_dogs_1.png");
	uiIcon1->setPosition(_screenSize.width * 230 / 1280, _screenSize.height * 57 / 720);
	uiIcon1->setScaleY(0.8f);
	uiIcon1->setTag(unit1);
	this->addChild(uiIcon1, kForeground);

	uiIcon2 = Sprite::createWithSpriteFrameName("unit_icon_dogs_2.png");
	uiIcon2->setPosition(_screenSize.width * 420 / 1280, _screenSize.height * 57 / 720);
	uiIcon2->setScaleY(0.8f);
	uiIcon2->setTag(unit2);
	this->addChild(uiIcon2, kForeground);

	uiIcon3 = Sprite::createWithSpriteFrameName("unit_icon_dogs_3.png");
	uiIcon3->setPosition(_screenSize.width * 617 / 1280, _screenSize.height * 57 / 720);
	uiIcon3->setScaleY(0.8f);
	uiIcon3->setTag(unit3);
	this->addChild(uiIcon3, kForeground);

	unitMask1 = Sprite::createWithSpriteFrameName("unitMask.png");
	unitMask1->setPosition(_screenSize.width * 230 / 1280, _screenSize.height * 57 / 720);
	unitMask1->setScaleY(0.8f);
	unitMask1->setVisible(true);
	this->addChild(unitMask1, kForeground);

	unitMask2 = Sprite::createWithSpriteFrameName("unitMask.png");
	unitMask2->setPosition(_screenSize.width * 420 / 1280, _screenSize.height * 57 / 720);
	unitMask2->setScaleY(0.8f);
	unitMask2->setVisible(true);
	this->addChild(unitMask2, kForeground);

	unitMask3 = Sprite::createWithSpriteFrameName("unitMask.png");
	unitMask3->setPosition(_screenSize.width * 617 / 1280, _screenSize.height * 57 / 720);
	unitMask3->setScaleY(0.8f);
	unitMask3->setVisible(true);
	this->addChild(unitMask3, kForeground);

	//Money
	moneyIcon = Sprite::createWithSpriteFrameName("coinpoint.png");
	moneyIcon->setPosition(_screenSize.width * 0.12f, _screenSize.height * 655 / 720);
	moneyIcon->setScaleY(0.8f);
	this->addChild(moneyIcon, kForeground);

	//Score�̹���
	scoreIcon = Sprite::createWithSpriteFrameName("dogspawnpoint.png");
	scoreIcon->setPosition(_screenSize.width * 0.46f, _screenSize.height * 655 / 720);
	scoreIcon->setScaleY(0.8f);
	this->addChild(scoreIcon, kForeground);

	//�Ͻ����� ������
	Sprite* pauseBtn = Sprite::createWithSpriteFrameName("pausebutton.png");
	pauseBtn->setPosition(_screenSize.width * 0.95f, _screenSize.height * 655 / 720);
	pauseBtn->setScaleY(0.8f);
	pauseBtn->setTag(pauseButton_tag);
	this->addChild(pauseBtn, kForeground);

	playerEmptyHP = Sprite::createWithSpriteFrameName("hp_back.png");
	playerEmptyHP->setPosition(120, 300);
	playerEmptyHP->setScaleY(0.8f);
	bg->addChild(playerEmptyHP, kForeground);

	amrmyEmptyHP = Sprite::createWithSpriteFrameName("hp_back.png");
	amrmyEmptyHP->setPosition(1800, 300);
	amrmyEmptyHP->setScaleY(0.8f);
	bg->addChild(amrmyEmptyHP, kForeground);

	playerFullHPbar = Sprite::create("doghp_middle.png", Rect(0, 0, (210 * playerHP / playerfullHP), 54));
	playerFullHPbar->setPosition(playerFullHPbar->getContentSize().width * 0.5f, playerFullHPbar->getContentSize().height * 0.5f);
	playerEmptyHP->addChild(playerFullHPbar, kForeground);

	playerFront = Sprite::createWithSpriteFrameName("doghp_front.png");
	playerFront->setPosition(120, 300);
	playerFront->setScaleY(0.8f);
	bg->addChild(playerFront, kForeground);

	amrmyFullHPbar = Sprite::create("cathp_middle.png", Rect(0, 0, (210 * amrmyHP / amrmyfullHP), 54));
	amrmyFullHPbar->setPosition(amrmyFullHPbar->getContentSize().width * 0.5f, amrmyFullHPbar->getContentSize().height * 0.5f);
	amrmyEmptyHP->addChild(amrmyFullHPbar, kForeground);

	amrmyFront = Sprite::createWithSpriteFrameName("cathp_front.png");
	amrmyFront->setPosition(1800, 300);
	amrmyFront->setScaleY(0.8f);
	bg->addChild(amrmyFront, kForeground);

	//�Ͻ����� �޴���..
	//background
	_popupwindow = Sprite::create("popupwindow.png");
	_popupwindow->setPosition(_screenSize.width * 0.5, _screenSize.height * 0.5);
	_popupwindow->setVisible(false);
	this->addChild(_popupwindow, kpopup);
	//pauseText
	_pauseText = Sprite::createWithSpriteFrameName("popupwindow_pause.png");
	_pauseText->setPosition(_screenSize.width * 0.5, _screenSize.height * 0.65);
	_pauseText->setVisible(false);
	this->addChild(_pauseText, kpopup);
	//menu
	_backMenuButton = Sprite::createWithSpriteFrameName("mainbackbutton.png");
	_backMenuButtonOn = Sprite::createWithSpriteFrameName("mainbackbuttonon.png");
	_continuButton = Sprite::createWithSpriteFrameName("exitno.png");
	_continuButtonOn = Sprite::createWithSpriteFrameName("exitnoon.png");
	_exitButton = Sprite::createWithSpriteFrameName("exityes.png");
	_exitButtonOn = Sprite::createWithSpriteFrameName("exityeson.png");

	MenuItemSprite* backMenuItem = MenuItemSprite::create(
		_backMenuButton,
		_backMenuButtonOn,
		CC_CALLBACK_1(GameLayer::_pauseMenuSeletion, this, 2));
	MenuItemSprite* continuItem = MenuItemSprite::create(
		_continuButton,
		_continuButtonOn,
		CC_CALLBACK_1(GameLayer::_pauseMenuSeletion, this, 1));

	MenuItemSprite* exitItem = MenuItemSprite::create(
		_exitButton,
		_exitButtonOn,
		CC_CALLBACK_1(GameLayer::_pauseMenuSeletion, this, 3));

	_pauseMenu = Menu::create(continuItem, backMenuItem, exitItem, NULL);
	_pauseMenu->setPosition(
		Point(_screenSize.width * 0.5f, _screenSize.height * 0.45));
	_pauseMenu->alignItemsHorizontallyWithPadding(35.0f);
	_pauseMenu->setVisible(false);
	addChild(_pauseMenu, kpopup);

	//HPbar
	setprogressbar();

	//���ھ�
	setScore();

	//�Ӵ�
	setMoney();

	//GameOver
	_gameClear = Sprite::create("gameclearscene.png");
	_gameClear->setPosition(Point(_screenSize.width * 0.5f, _screenSize.height * 0.5f));
	_gameClear->setVisible(false);
	this->addChild(_gameClear, kForeground);

	_gameOver = Sprite::create("gameoverscene.png");
	_gameOver->setPosition(Point(_screenSize.width * 0.5f, _screenSize.height * 0.5f));
	_gameOver->setVisible(false);

	this->addChild(_gameOver, kForeground);

	createAction();

}

// ��� ��ƼŬ
void GameLayer::createParticles()
{
	// �� ��ƼŬ ȿ�� �ʱ�ȭ
	_snow = ParticleSystemQuad::create("snow.plist");
	_snow->setPosition(Point(_screenSize.width * 0.2f, _screenSize.height * 0.8f));
	this->addChild(_snow, kBackground);

	_snow2 = ParticleSystemQuad::create("snow.plist");
	_snow2->setPosition(Point(_screenSize.width * 0.8f, _screenSize.height * 0.8f));
	this->addChild(_snow2, kBackground);

	_sun = ParticleSystemQuad::create("sun.plist");
	_sun->setPosition(Point(_screenSize.width * 0.15f, _screenSize.height * 0.8f));
	this->addChild(_sun, kBackground);

}

////////////////////////////////////////UI����///////////////////////////////////////////////////
// HP��
void GameLayer::setprogressbar()
{
	playerEmptyHP->removeChild(playerFullHPbar, true);
	amrmyEmptyHP->removeChild(amrmyFullHPbar, true);

	playerFullHPbar = Sprite::create("doghp_middle.png", Rect(0, 0, (210 * playerHP / playerfullHP), 54));
	playerFullHPbar->setPosition(playerFullHPbar->getContentSize().width * 0.5f, playerFullHPbar->getContentSize().height * 0.5f);
	playerEmptyHP->addChild(playerFullHPbar, kForeground);

	amrmyFullHPbar = Sprite::create("cathp_middle.png", Rect(0, 0, (210 * amrmyHP / amrmyfullHP), 54));
	amrmyFullHPbar->setPosition(amrmyFullHPbar->getContentSize().width * 0.5f, amrmyFullHPbar->getContentSize().height * 0.5f);
	amrmyEmptyHP->addChild(amrmyFullHPbar, kForeground);
}

// ���ھ�
void GameLayer::setScore()
{
	CCSize wisibleSize = CCDirector::sharedDirector()->getVisibleSize();
	CCPoint origin = CCDirector::sharedDirector()->getVisibleOrigin();
	char value[100] = { 0 };
	sprintf(value, "%i", score);
	scoreFont = CCLabelBMFont::create(value, "bmpfont.fnt");
	scoreFont->setPosition(_screenSize.width * 0.5f, _screenSize.height * 0.905f);
	this->addChild(scoreFont, kForeground);
	scoreFont->retain();
}

// �� ����
void GameLayer::setMoney()
{
	CCSize wisibleSize = CCDirector::sharedDirector()->getVisibleSize();
	CCPoint origin = CCDirector::sharedDirector()->getVisibleOrigin();
	char value[100] = { 0 };
	sprintf(value, "%i", money);
	moneyFont = CCLabelBMFont::create(value, "bmpfont.fnt");
	moneyFont->setPosition(_screenSize.width * 0.15f, _screenSize.height * 0.9f);
	this->addChild(moneyFont, kForeground);
	moneyFont->retain();
}

void GameLayer::_pauseMenuSeletion(Ref* sender, int num)
{
	SimpleAudioEngine::getInstance()->playEffect("opengameclick.wav");
	Director::sharedDirector()->resume();

	switch (num)
	{
	case 1:
	{
		_popupwindow->setVisible(false);
		_pauseText->setVisible(false);
		_pauseMenu->setVisible(false);
		break;
	}
	case 2:
	{
		SimpleAudioEngine::getInstance()->stopBackgroundMusic();
		Scene* newScene = TransitionShrinkGrow::create(0.6f, MainLayer::createScene());
		Director::getInstance()->replaceScene(newScene);
		break;
	}
	case 3:
	{
		Director::getInstance()->end();	exit(0);
		break;
	}
	default:
		break;
	}
}

void GameLayer::update(float dt)
{
	if (bg->getPositionX() < _screenSize.width * 320 / 1280)
	{
		bg->setPosition(_screenSize.width * 320 / 1280, _screenSize.height * 410 / 720);
	}
	if (bg->getPositionX() > _screenSize.width * 960 / 1280)
	{
		bg->setPosition(_screenSize.width * 960 / 1280, _screenSize.height * 410 / 720);
	}

	// �����(enemy) ���� Ÿ�̸�
	_catTimer += dt;
	if (_catTimer > _catInterval)
	{
		_catTimer = 0;
		if (score >= 0)
		{
			SimpleAudioEngine::getInstance()->playEffect("cat04.mp3", false);
			Cat();
		}
	}

	// �� ���� Ÿ�̸�
	moneyTime += dt;
	if (moneyTime >= moneyRisenTime)
	{
		money += 1;
		char value[100] = { 0 };
		sprintf(value, "%i", money);
		moneyFont->setString(value);
		moneyTime = 0;
	}
	if (money >= 100)
	{
		unitMask1->setVisible(false);
	}
	else
	{
		unitMask1->setVisible(true);
	}

	unitCollision();

	_gameOvers();

}

///////////////////////////////////////////////////////////////////
//�ִϸ��̼� ���� �Լ�
Animation* unitAnimation(char spriteName[100], int animationType)
{
	auto animation = Animation::create();
	SpriteFrame* frame;
	int i = animationType;
	for (i = 0; i <= 1; i++)
	{
		char szName[100] = { 0 };
		sprintf(szName, "%s_%i.png", spriteName, i);
		frame = SpriteFrameCache::getInstance()->getSpriteFrameByName(szName);
		animation->addSpriteFrame(frame);
	}
	animation->setDelayPerUnit(0.5f / 3.0f);
	animation->setRestoreOriginalFrame(false);
	animation->setLoops(-1);

	return animation;
}

void GameLayer::createAction()
{
	CCBlink* blinkUnit = CCBlink::create(1.0f, 3);
	auto dieAnimation = Sequence::create(blinkUnit, FadeOut::create(1.8f), //������ 0���� ��
		CallFunc::create(CC_CALLBACK_0(GameLayer::dieDone, this)), NULL);
	dieAnimation->retain();

	ScaleTo* effectScale1 = ScaleTo::create(0.3f, 1.3f, 1.3f * 0.8f);
	ScaleTo* effectScale2 = ScaleTo::create(0.1f, 0.9f, 0.9f * 0.8f);
	effectAnimation = Sequence::create(effectScale1, effectScale2, CallFunc::create(CC_CALLBACK_0(GameLayer::eraseEffect, this)), NULL);
	effectAnimation->retain();
}

void GameLayer::eraseEffect()
{
	effectImg->setVisible(false);
}

//////////////////////////////// ���� ���� ////////////////////////
// ������ ����
void GameLayer::createPools()
{
	Sprite* dog01;

	Sprite* cat01;

	// ������1
	_dogPoolIndex = 0;
	for (int i = 0; i < poolMax; i++)
	{
		DogUnit* dogunit = DogUnit::creat("dog1");
		dogunit->setVisible(false);
		bg->addChild(dogunit, kForeground);
		_dogPool.pushBack(dogunit);
	}

	// ����� 1
	_catPoolIndex = 0;
	for (int i = 0; i < poolMax; i++)
	{
		CatUnit* catunit = CatUnit::creat("cat01");
		catunit->setVisible(false);
		bg->addChild(catunit, kForeground);
		_catPool.pushBack(catunit);
	}
}

// ������ ����
void GameLayer::Dog()
{
	SimpleAudioEngine::getInstance()->playEffect("dog05.mp3", false);
	if (_dog01Objects.size() > 30)
	{
		return;
	}

	dog01All = _dogPool.at(_dogPoolIndex);
	_dogPoolIndex++;

	if (_dogPoolIndex == _dogPool.size())
	{
		_dogPoolIndex = 0;
	}

	dog01All->setVisible(true);
	dog01All->setPosition(_screenSize.width * 0.1f, _screenSize.height * 0.2f);
	auto _dogAnimate = Animate::create(unitAnimation("dog1", WALKING));
	dogmove = MoveBy::create(12.0, Point(1920, 0));
	dog01All->runAction(dogmove);
	dog01All->runAction(_dogAnimate);
	_dog01Objects.pushBack(dog01All);
}

// ����� ����
void GameLayer::Cat()
{
	if (_cat01Objects.size() > 30)
	{
		return;
	}

	cat01All = _catPool.at(_catPoolIndex);
	_catPoolIndex++;

	if (_catPoolIndex == _catPool.size())
	{
		_catPoolIndex = 0;
	}

	cat01All->setVisible(true);
	cat01All->setPosition(catHouse->getPositionX(), _screenSize.height * 0.18f);
	catmove = MoveBy::create(12.0, Point(-1920, 0));
	auto _dogAnimate = Animate::create(unitAnimation("cat01", WALKING));
	cat01All->runAction(catmove);
	cat01All->runAction(_dogAnimate);
	_cat01Objects.pushBack(cat01All);
}

void GameLayer::_gameOvers()
{
	if (gameover)
	{
		_gameOver->setVisible(true);
		this->scheduleOnce(schedule_selector(GameLayer::_newScene), 5.0f);
	}
	else if (gameclear)
	{
		_gameClear->setVisible(true);
		this->scheduleOnce(schedule_selector(GameLayer::_newScene), 5.0f);
	}
}

void GameLayer::_newScene(float dt)
{
	Scene* newScene = TransitionFadeBL::create(0.6f, MainLayer::createScene());
	Director::getInstance()->replaceScene(newScene);
}

////////////////////////////////// ��ġ ////////////////////////////////////////
void GameLayer::onTouchesBegan(const std::vector<Touch*>& touches, Event* event)
{
	Touch* touch = touches.at(0);
	Point tap = touch->getLocation();
	// ����01 ��ġ ����
	auto uiIcon1 = (Sprite*)this->getChildByTag(unit1);
	auto pauseBtn = (Sprite*)this->getChildByTag(pauseButton_tag);
	Rect dog01t = uiIcon1->getBoundingBox();
	Rect pauseBtnt = pauseBtn->getBoundingBox();

	if (dog01t.containsPoint(tap) && money > 100)
	{
		Dog();
		money -= 150;
	}

	if (pauseBtnt.containsPoint(tap))
	{
		Director::sharedDirector()->pause();
		_popupwindow->setVisible(true);
		_pauseText->setVisible(true);
		_pauseMenu->setVisible(true);
	}
}

void GameLayer::onTouchesMoved(const std::vector<Touch*>& touches, Event* event)
{
	if (_gamePause) { return; }
	Touch* touch = touches.at(0);
	Point tap = touch->getLocation();
	Vec2 touchlenght = touch->getPreviousLocation() - touch->getLocation();
	if (touchlenght.x > 50 && touchlenght.x < 150)
	{
		auto ActionTo = MoveTo::create(0.5f, Point(bg->getPositionX() - _screenSize.width * 100 / 1280, _screenSize.height * 410 / 720));
		bg->runAction(ActionTo);
	}
	else if (touchlenght.x >= 150)
	{
		auto ActionTo = MoveTo::create(0.5f, Point(bg->getPositionX() - _screenSize.width * 400 / 1280, _screenSize.height * 410 / 720));
		bg->runAction(ActionTo);
	}
	if (touchlenght.x < -50 && touchlenght.x > -150)
	{
		auto ActionTo = MoveTo::create(0.5f, Point(bg->getPositionX() + _screenSize.width * 100 / 1280, _screenSize.height * 410 / 720));
		bg->runAction(ActionTo);
	}
	else if (touchlenght.x <= -150)
	{
		auto ActionTo = MoveTo::create(0.5f, Point(bg->getPositionX() + _screenSize.width * 400 / 1280, _screenSize.height * 410 / 720));
		bg->runAction(ActionTo);
	}
}