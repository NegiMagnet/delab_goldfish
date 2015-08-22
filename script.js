// 画面サイズ(端末の縦横比によって変える)
var SCREEN_WIDTH;
var SCREEN_HEIGHT;

// フィールドサイズ
var FIELD_WIDTH = 1600;
var FIELD_HEIGHT = 1600;

// AI魚の数
var FISH_NUMS = 10;

var clamp = function(l, c, r) {
	return Math.min(Math.max(l, c), r);
};

var SRC_PATH = "./src/";

var ASSETS = {
	"title" : SRC_PATH + "title.png",
	"fish" : SRC_PATH + "fish.png",
	"field" : SRC_PATH + "field.png",
	"tutorial" : SRC_PATH + "tutorial.png",
	"poi" : SRC_PATH + "poi.png",
};

// エントリポイント
tm.main(function() {
	if(window.innerHeight < window.innerWidth) {
		SCREEN_HEIGHT = 800;
		SCREEN_WIDTH  = SCREEN_HEIGHT * window.innerWidth / window.innerHeight;
	} else {
		SCREEN_WIDTH  = 800;
		SCREEN_HEIGHT = SCREEN_WIDTH * window.innerHeight / window.innerWidth;
	}
//	alert(window.innerWidth + " " + window.innerHeight);

	var app = tm.display.CanvasApp("#world");
	app.resize(SCREEN_WIDTH, SCREEN_HEIGHT);
	app.fitWindow();

	var ScLoadAsset = tm.ui.LoadingScene( {
		assets: ASSETS,
		width: SCREEN_WIDTH,
		height: SCREEN_HEIGHT,
		nextScene: SceneTitle
//		nextScene: SceneMain
	});

	app.replaceScene(ScLoadAsset);
	app.run();
});

// MainScene定義
tm.define("SceneMain", {
	
	superClass: "tm.app.Scene",

	init: function() {
		this.superInit();

		this.field = new Field();
		this.field.addChildTo(this);
		this.field.setPositionLU(0, 0);

		this.fish = new Fish();
		this.fish.addChildTo(this);
		this.fish.posx = SCREEN_WIDTH/2;
		this.fish.posy = SCREEN_HEIGHT/2;
		this.fish.velx = 0.0;
		this.fish.vely = 0.0;
		this.fish.setPosition(this.fish.posx, this.fish.posy);

		this.fishAI = new Array(FISH_NUMS);
		for(i=0; i<FISH_NUMS; i++) {
			this.fishAI[i] = new FishAI();
			this.fishAI[i].addChildTo(this);
			this.fishAI[i].posx = FIELD_WIDTH * Math.random();
			this.fishAI[i].posy = FIELD_HEIGHT * Math.random();
		}

		this.addEventListener("pointingmove", function(e) {
			// タッチ位置の方向にプレイヤーを動かす
			v = tm.geom.Vector2(
				e.app.pointing.x - this.fish.getScrX(),
				e.app.pointing.y - this.fish.getScrY()
			);
			this.fish.move(
				v.normalize().dot(tm.geom.Vector2.RIGHT)/2,
				v.normalize().dot(tm.geom.Vector2.UP)/2
			);
		});

		// ブラックアウトからの復帰演出
		var rect = tm.display.RectangleShape(SCREEN_WIDTH, SCREEN_HEIGHT)
			.addChildTo(this)
			.setPosition(SCREEN_WIDTH/2, SCREEN_HEIGHT/2)
			.setAlpha(1.0);
		rect.canvas.clearColor("black");
		rect.tweener.clear()
			.to({alpha:0.0}, 500);
	},

	update: function(app) {
		this.fish.update();
		for(i=0; i<FISH_NUMS; i++) {
			this.fishAI[i].update();
		}

		var fx = -clamp(0, this.fish.posx - SCREEN_WIDTH/2,  FIELD_WIDTH - SCREEN_WIDTH);
		var fy = -clamp(0, this.fish.posy - SCREEN_HEIGHT/2, FIELD_HEIGHT - SCREEN_HEIGHT);
		this.field.setPositionLU(fx, fy);
		this.fish.setPos();

		for(i=0; i<FISH_NUMS; i++) {
			var ai_px = this.fish.getScrX() + this.fishAI[i].posx - this.fish.posx;
			var ai_py = this.fish.getScrY() + this.fishAI[i].posy - this.fish.posy;
			this.fishAI[i].setPos(ai_px, ai_py);
		}
	}
});

// 金魚
tm.define("Fish", {

	superClass: "tm.app.AnimationSprite",

	posx : 0,
	posy : 0,
	velx : 0,
	vely : 0,

	init: function() {
		var ss = tm.asset.SpriteSheet({
			image : "fish",
			frame: {
				width : 128,
				height : 128,
				count : 2
			},
			animations: {
				"lv1" : [0, 1, "lv1"],
				"lv2" : [1,0, "lv2"],
			}
		});
		this.superInit(ss, 128, 128);
		this.gotoAndPlay("lv1");
	},

	update: function() {
		this.velx *= 0.95;
		this.vely *= 0.95;
		this.posx += this.velx;
		this.posy += this.vely;

		if( this.posx < 0 ) this.posx = 0;
		else if( FIELD_WIDTH <= this.posx ) this.posx = FIELD_WIDTH-1;
		if( this.posy < 0 ) this.posy = 0;
		else if( FIELD_HEIGHT <= this.posy ) this.posy = FIELD_HEIGHT-1;
	},

	setPos : function() {
		this.setPosition(this.getScrX(), this.getScrY());
		deg = tm.geom.Vector2(this.vely, -this.velx).toAngle() * 180.0 / Math.PI;
		deg = (deg+540.0) % 360.0;
		this.setRotation(deg);
	},

	move : function(ax, ay) {
		this.velx += ax;
		this.vely += ay;
	},

	getScrX : function() {
		if( this.posx < SCREEN_WIDTH/2 ) {
			return this.posx;
		} else if( this.posx < FIELD_WIDTH-SCREEN_WIDTH/2 ) {
			return SCREEN_WIDTH/2;
		} else {
			return this.posx - FIELD_WIDTH + SCREEN_WIDTH;
		}
	},

	getScrY : function() {
		if( this.posy < SCREEN_HEIGHT/2 ) {
			return this.posy;
		} else if( this.posy < FIELD_HEIGHT-SCREEN_HEIGHT/2 ) {
			return SCREEN_HEIGHT/2;
		} else {
			return this.posy - FIELD_HEIGHT + SCREEN_HEIGHT;
		}
	}

});

// 金魚AI
tm.define("FishAI", {
	superClass : "tm.app.AnimationSprite",

	posx : 0,
	posy : 0,
	velx : 0,
	vely : 0,
	accx : 0,
	accy : 0,

	INTERVAL_ACCEL : 30,
	INTERVAL_STOP : 80,
	interval : 0,

	init : function() {
		var ss = tm.asset.SpriteSheet({
			image : "fish",
			frame: {
				width : 128,
				height : 128,
				count : 2
			},
			animations: {
				"lv1" : [0, 1, "lv1"],
				"lv2" : [1,0, "lv2"],
			}
		});
		this.superInit(ss, 128, 128);
		this.gotoAndPlay("lv1");
		this.interval = Math.floor((this.INTERVAL_ACCEL + this.INTERVAL_STOP) * Math.random());
	},

	update : function() {

		// decide accel direction
		if( this.interval == 0 ) {
			var rad = Math.random() * (2.0*Math.PI);
			this.accx = Math.cos(rad)/2;
			this.accy = Math.sin(rad)/2;
		}

		if( this.interval < this.INTERVAL_ACCEL ) {
			// accel
			this.velx += this.accx;
			this.vely += this.accy;
		} else if( this.interval+1 == this.INTERVAL_ACCEL + this.INTERVAL_STOP ) {
			// re-accel
			this.interval = -1;
		}
		this.interval++;

		this.velx *= 0.95;
		this.vely *= 0.95;
		this.posx += this.velx;
		this.posy += this.vely;

		if( this.posx < 0 ) this.posx = 0;
		else if( FIELD_WIDTH <= this.posx ) this.posx = FIELD_WIDTH-1;
		if( this.posy < 0 ) this.posy = 0;
		else if( FIELD_HEIGHT <= this.posy ) this.posy = FIELD_HEIGHT-1;
	},

	setPos : function(px, py) {
		this.setPosition(px, py);
		deg = tm.geom.Vector2(this.vely, -this.velx).toAngle() * 180.0 / Math.PI;
		deg = (deg+540.0) % 360.0;
		this.setRotation(deg);
	}
});

// フィールド
tm.define("Field", {
	superClass : "tm.app.Sprite",

	scalex : 1,
	scaley : 1,

	init : function() {
		this.superInit("field"); // field.png, 400x400
		var size = this.getBoundingRect();
		this.scalex = FIELD_WIDTH / size.width;
		this.scaley = FIELD_HEIGHT / size.height;
		this.setScale(this.scalex, this.scaley);
	},

	// 左上座標で位置セット
	setPositionLU : function(x, y) {
		var size = this.getBoundingRect();
		var nx = Math.max(0, Math.min(x+FIELD_WIDTH/2, FIELD_WIDTH-SCREEN_WIDTH/2));
		var ny = Math.max(0, Math.min(y+FIELD_HEIGHT/2, FIELD_HEIGHT-SCREEN_HEIGHT/2));
		this.setPosition(nx, ny);
	},
});

// タイトル
tm.define("SceneTitle", {
	superClass : "tm.app.Scene",

	init : function() {
		this.superInit();

		// タイトル絵表示
		this.spTitle = new tm.app.Sprite("title");
		this.spTitle.addChildTo(this);
		this.spTitle.setPosition(SCREEN_WIDTH/2, SCREEN_HEIGHT/2);
		this.spTitle.setScale(SCREEN_WIDTH/this.spTitle.getBoundingRect().width, SCREEN_HEIGHT/this.spTitle.getBoundingRect().height);

/*
		var uiTitle = {
			main : {
				children : [{
					type : "Label",
					name : "timeLabel",
					x : SCREEN_WIDTH/2,
					y : SCREEN_HEIGHT/3,
					fillStyle : "white",
					text : "TITLE",
					fontSize : 40,
					align : "center"
				}]
			}
		};
		this.fromJSON(uiTitle.main);
*/
	},

	onpointingstart : function(e) {
		e.app.replaceScene(SceneTutorial());
	},
});

// チュートリアル
tm.define("SceneTutorial", {
	superClass : "tm.app.Scene",

	init : function() {
		this.superInit();

		// ルール説明1枚絵表示
		this.spTutorial = new tm.app.Sprite("tutorial");
		this.spTutorial.addChildTo(this);
		this.spTutorial.setPosition(SCREEN_WIDTH/2, SCREEN_HEIGHT/2);
		this.spTutorial.setScale(SCREEN_WIDTH/this.spTutorial.getBoundingRect().width, SCREEN_HEIGHT/this.spTutorial.getBoundingRect().height);
	},

	onpointingstart : function(e) {
		var rect = tm.display.RectangleShape(SCREEN_WIDTH, SCREEN_HEIGHT)
			.addChildTo(this)
			.setPosition(SCREEN_WIDTH/2, SCREEN_HEIGHT/2)
			.setAlpha(0.0);
		rect.canvas.clearColor("black");
		rect.tweener.clear()
			.to({alpha:1.0}, 500)
			.call(function() { e.app.replaceScene(SceneMain()); });
	},
});

// Ready
tm.define("SceneReady", {
	superClass : "tm.app.Scene",

	init : function() {
		this.superInit();

		// TODO : カウントダウン
	},

	onpointingstart : function(e) {
		e.app.popScene();
	},
});


