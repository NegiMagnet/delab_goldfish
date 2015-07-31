// 画面サイズ
var SCREEN_WIDTH  = 800;
var SCREEN_HEIGHT = 800;

// フィールドサイズ
var FIELD_WIDTH = 1600;
var FIELD_HEIGHT = 1600;

// AI魚の数
var FISH_NUMS = 10;

var clamp = function(l, c, r) {
	return Math.min(Math.max(l, c), r);
};

var ASSETS = {
	"fish" : "./src/fish.png",
	"field" : "./src/field.png",
};

tm.main(function() {
	var app = tm.display.CanvasApp("#world");
	app.resize(SCREEN_WIDTH, SCREEN_HEIGHT);
	app.fitWindow();

	var ScLoadAsset = tm.ui.LoadingScene( {
		assets: ASSETS,
		width: SCREEN_WIDTH,
		height: SCREEN_HEIGHT,
		nextScene: MainScene
	});

	app.replaceScene(ScLoadAsset);
	app.run();
});

// MainScene定義
tm.define("MainScene", {
	
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

	superClass: "tm.app.Sprite",

	posx : 0,
	posy : 0,
	velx : 0,
	vely : 0,

	init: function() {
		this.superInit("fish");
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
	superClass : "tm.app.Sprite",

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
		this.superInit("fish");
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

