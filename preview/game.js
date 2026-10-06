import * as THREE from 'three';

// --- GAME STATE ---
const state = {
    lives: 5,
    coins: 0,
    score: 0,
    time: 300,
    isPaused: false,
    gameOver: false,
    courseClear: false,
    powerup: 'normal', // 'normal', 'fire', 'mega'
    megaTimer: 0,
    invincibleTimer: 0
};

// --- INITIALIZE THREE.JS SCENE ---
const container = document.getElementById('canvas-container');
const scene = new THREE.Scene();
scene.background = new THREE.Color(0x60a5fa); // Mario sky blue
scene.fog = new THREE.FogExp2(0x60a5fa, 0.012);

const camera = new THREE.PerspectiveCamera(60, window.innerWidth / window.innerHeight, 0.1, 1000);
camera.position.set(0, 7, -12);

const renderer = new THREE.WebGLRenderer({ antialias: true });
renderer.setSize(window.innerWidth, window.innerHeight);
renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
renderer.shadowMap.enabled = true;
renderer.shadowMap.type = THREE.PCFSoftShadowMap;
container.appendChild(renderer.domElement);

// --- LIGHTING ---
const ambientLight = new THREE.AmbientLight(0xffffff, 0.65);
scene.add(ambientLight);

const sunLight = new THREE.DirectionalLight(0xfffaed, 1.2);
sunLight.position.set(25, 45, -20);
sunLight.castShadow = true;
sunLight.shadow.mapSize.width = 2048;
sunLight.shadow.mapSize.height = 2048;
sunLight.shadow.camera.near = 0.5;
sunLight.shadow.camera.far = 150;
const d = 35;
sunLight.shadow.camera.left = -d;
sunLight.shadow.camera.right = d;
sunLight.shadow.camera.top = d;
sunLight.shadow.camera.bottom = -d;
scene.add(sunLight);

// Hemisphere fill light
const hemiLight = new THREE.HemisphereLight(0x70c5ff, 0x3d7328, 0.4);
scene.add(hemiLight);

// --- PROCEDURAL TEXTURES & MATERIALS ---
function createBlockTexture(type) {
    const canvas = document.createElement('canvas');
    canvas.width = 128;
    canvas.height = 128;
    const ctx = canvas.getContext('2d');

    if (type === 'question') {
        ctx.fillStyle = '#ffb300';
        ctx.fillRect(0, 0, 128, 128);
        ctx.strokeStyle = '#c68400';
        ctx.lineWidth = 8;
        ctx.strokeRect(4, 4, 120, 120);

        // Corner rivets
        ctx.fillStyle = '#c68400';
        ctx.beginPath();
        ctx.arc(14, 14, 4, 0, Math.PI * 2);
        ctx.arc(114, 14, 4, 0, Math.PI * 2);
        ctx.arc(14, 114, 4, 0, Math.PI * 2);
        ctx.arc(114, 114, 4, 0, Math.PI * 2);
        ctx.fill();

        // Question mark '?'
        ctx.fillStyle = '#ffffff';
        ctx.font = 'bold 70px "Segoe UI", Arial, sans-serif';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText('?', 64, 60);
    } else if (type === 'empty') {
        ctx.fillStyle = '#a16207';
        ctx.fillRect(0, 0, 128, 128);
        ctx.strokeStyle = '#713f12';
        ctx.lineWidth = 6;
        ctx.strokeRect(4, 4, 120, 120);
    } else if (type === 'brick') {
        ctx.fillStyle = '#b45309';
        ctx.fillRect(0, 0, 128, 128);
        ctx.strokeStyle = '#78350f';
        ctx.lineWidth = 4;
        ctx.strokeRect(0, 0, 128, 64);
        ctx.strokeRect(0, 64, 128, 64);
        ctx.strokeRect(64, 0, 0, 64);
        ctx.strokeRect(32, 64, 0, 64);
        ctx.strokeRect(96, 64, 0, 64);
    }
    const tex = new THREE.CanvasTexture(canvas);
    return tex;
}

const materials = {
    marioRed: new THREE.MeshStandardMaterial({ color: 0xe52521, roughness: 0.4 }),
    marioBlue: new THREE.MeshStandardMaterial({ color: 0x1d4ed8, roughness: 0.5 }),
    marioWhite: new THREE.MeshStandardMaterial({ color: 0xf8fafc, roughness: 0.3 }),
    marioSkin: new THREE.MeshStandardMaterial({ color: 0xfbcfe8, roughness: 0.6 }),
    marioBrown: new THREE.MeshStandardMaterial({ color: 0x6d4c41, roughness: 0.7 }),
    marioYellow: new THREE.MeshStandardMaterial({ color: 0xfacc15, roughness: 0.3 }),
    grass: new THREE.MeshStandardMaterial({ color: 0x4ade80, roughness: 0.8 }),
    dirt: new THREE.MeshStandardMaterial({ color: 0x92400e, roughness: 0.9 }),
    question: new THREE.MeshStandardMaterial({ map: createBlockTexture('question'), roughness: 0.3 }),
    emptyBlock: new THREE.MeshStandardMaterial({ map: createBlockTexture('empty'), roughness: 0.6 }),
    brick: new THREE.MeshStandardMaterial({ map: createBlockTexture('brick'), roughness: 0.5 }),
    pipeGreen: new THREE.MeshStandardMaterial({ color: 0x15803d, roughness: 0.2, metalness: 0.1 }),
    gold: new THREE.MeshStandardMaterial({ color: 0xffd700, metalness: 0.8, roughness: 0.2 }),
    cloud: new THREE.MeshStandardMaterial({ color: 0xffffff, roughness: 0.9 }),
    goombaBrown: new THREE.MeshStandardMaterial({ color: 0x8b4513, roughness: 0.7 }),
    goombaFeet: new THREE.MeshStandardMaterial({ color: 0x271711, roughness: 0.8 }),
    koopaGreen: new THREE.MeshStandardMaterial({ color: 0x22c55e, roughness: 0.3 }),
    koopaYellow: new THREE.MeshStandardMaterial({ color: 0xfef08a, roughness: 0.5 })
};

// --- BUILD 3D MARIO CHARACTER MODEL ---
function createMarioModel() {
    const marioGroup = new THREE.Group();

    // Root model container for squash/stretch tweens
    const bodyContainer = new THREE.Group();
    marioGroup.add(bodyContainer);

    // Torso (Overalls)
    const torsoGeo = new THREE.CylinderGeometry(0.38, 0.34, 0.65, 12);
    const torso = new THREE.Mesh(torsoGeo, materials.marioBlue);
    torso.position.y = 0.65;
    torso.castShadow = true;
    bodyContainer.add(torso);

    // Red/White shirt inner
    const shirtGeo = new THREE.SphereGeometry(0.36, 12, 12);
    const shirt = new THREE.Mesh(shirtGeo, materials.marioRed);
    shirt.position.y = 0.8;
    shirt.scale.set(1, 0.7, 0.9);
    shirt.castShadow = true;
    bodyContainer.add(shirt);

    // Yellow buttons on overalls
    const buttonGeo = new THREE.CylinderGeometry(0.04, 0.04, 0.05, 8);
    const bLeft = new THREE.Mesh(buttonGeo, materials.marioYellow);
    bLeft.rotation.x = Math.PI / 2;
    bLeft.position.set(-0.16, 0.78, 0.32);
    bodyContainer.add(bLeft);

    const bRight = bLeft.clone();
    bRight.position.x = 0.16;
    bodyContainer.add(bRight);

    // Head
    const headGroup = new THREE.Group();
    headGroup.position.y = 1.25;

    const faceGeo = new THREE.SphereGeometry(0.32, 16, 16);
    const face = new THREE.Mesh(faceGeo, materials.marioSkin);
    face.castShadow = true;
    headGroup.add(face);

    // Nose
    const noseGeo = new THREE.SphereGeometry(0.12, 12, 12);
    const nose = new THREE.Mesh(noseGeo, materials.marioSkin);
    nose.position.set(0, 0.02, 0.32);
    nose.scale.set(1.2, 0.9, 1);
    headGroup.add(nose);

    // Mustache
    const stacheGeo = new THREE.BoxGeometry(0.36, 0.09, 0.08);
    const stache = new THREE.Mesh(stacheGeo, materials.marioBrown);
    stache.position.set(0, -0.06, 0.32);
    headGroup.add(stache);

    // Cap
    const capGeo = new THREE.SphereGeometry(0.34, 16, 16, 0, Math.PI * 2, 0, Math.PI * 0.55);
    const cap = new THREE.Mesh(capGeo, materials.marioRed);
    cap.position.y = 0.08;
    headGroup.add(cap);

    // Cap brim
    const brimGeo = new THREE.CylinderGeometry(0.38, 0.38, 0.04, 16, 1, false, 0, Math.PI);
    const brim = new THREE.Mesh(brimGeo, materials.marioRed);
    brim.position.set(0, 0.12, 0.2);
    brim.rotation.x = 0.15;
    headGroup.add(brim);

    bodyContainer.add(headGroup);

    // Arms & Hands
    const armGeo = new THREE.CylinderGeometry(0.1, 0.09, 0.45, 8);
    const leftArm = new THREE.Mesh(armGeo, materials.marioRed);
    leftArm.position.set(-0.46, 0.68, 0);
    leftArm.rotation.z = 0.3;
    bodyContainer.add(leftArm);

    const rightArm = leftArm.clone();
    rightArm.position.x = 0.46;
    rightArm.rotation.z = -0.3;
    bodyContainer.add(rightArm);

    const gloveGeo = new THREE.SphereGeometry(0.12, 8, 8);
    const leftHand = new THREE.Mesh(gloveGeo, materials.marioWhite);
    leftHand.position.set(-0.55, 0.44, 0);
    bodyContainer.add(leftHand);

    const rightHand = leftHand.clone();
    rightHand.position.x = 0.55;
    bodyContainer.add(rightHand);

    // Legs & Shoes
    const legGeo = new THREE.CylinderGeometry(0.13, 0.12, 0.42, 8);
    const leftLeg = new THREE.Mesh(legGeo, materials.marioBlue);
    leftLeg.position.set(-0.2, 0.25, 0);
    bodyContainer.add(leftLeg);

    const rightLeg = leftLeg.clone();
    rightLeg.position.x = 0.2;
    bodyContainer.add(rightLeg);

    const shoeGeo = new THREE.BoxGeometry(0.24, 0.18, 0.4);
    const leftShoe = new THREE.Mesh(shoeGeo, materials.marioBrown);
    leftShoe.position.set(-0.2, 0.08, 0.06);
    leftShoe.castShadow = true;
    bodyContainer.add(leftShoe);

    const rightShoe = leftShoe.clone();
    rightShoe.position.x = 0.2;
    bodyContainer.add(rightShoe);

    return {
        root: marioGroup,
        bodyContainer,
        headGroup,
        shirt,
        cap,
        brim,
        leftArm,
        rightArm,
        leftLeg,
        rightLeg,
        leftHand,
        rightHand
    };
}

const mario = createMarioModel();
scene.add(mario.root);
mario.root.position.set(0, 0, 0);

// --- MARIO CONTROLLER PHYSICS & STATE ---
const player = {
    pos: mario.root.position,
    vel: new THREE.Vector3(),
    speed: 13,
    jumpForce: 16.5,
    grounded: false,
    jumpCount: 0,
    lastJumpTime: 0,
    coyoteTimer: 0,
    jumpBufferTimer: 0,
    isCrouching: false,
    isGroundPounding: false,
    groundPoundState: 'none', // 'pause', 'fall', 'impact'
    facingAngle: 0,
    walkAnimPhase: 0,
    squashTimer: 0
};

// --- WORLD GENERATION: PLATFORMS, BLOCKS, PIPES, ENEMIES ---
const platforms = [];
const blocks = [];
const coins = [];
const enemies = [];
const shells = [];
const fireballs = [];
const particles = [];
let flagpoleObj = null;

// Helper: Add static collision box
function addPlatform(x, y, z, w, h, d, mat) {
    const geo = new THREE.BoxGeometry(w, h, d);
    const mesh = new THREE.Mesh(geo, mat);
    mesh.position.set(x, y, z);
    mesh.receiveShadow = true;
    scene.add(mesh);

    const box = new THREE.Box3().setFromObject(mesh);
    platforms.push({ mesh, box, w, h, d });
    return mesh;
}

// Main Ground & Hills
addPlatform(0, -1, 35, 24, 2, 85, materials.grass);

// Underground bonus cave platform
const caveGround = addPlatform(0, -25, 30, 26, 2, 35, materials.dirt);

// Floating Platforms
addPlatform(0, 4.5, 18, 6, 0.8, 6, materials.grass);
addPlatform(-5, 8.5, 30, 5, 0.8, 5, materials.grass);
addPlatform(5, 12.5, 42, 5, 0.8, 5, materials.grass);

// Question Mark (?) Block class
class QuestionBlock {
    constructor(x, y, z, itemType = 'coin') {
        this.initialY = y;
        this.itemType = itemType;
        this.used = false;
        this.bounceProgress = 0;

        const geo = new THREE.BoxGeometry(1.6, 1.6, 1.6);
        this.mesh = new THREE.Mesh(geo, materials.question);
        this.mesh.position.set(x, y, z);
        this.mesh.castShadow = true;
        this.mesh.receiveShadow = true;
        scene.add(this.mesh);

        this.box = new THREE.Box3().setFromObject(this.mesh);
        blocks.push(this);
    }

    hit() {
        if (this.used) return;
        this.used = true;
        this.mesh.material = materials.emptyBlock;
        this.bounceProgress = 1.0;

        window.soundEngine.playBlockBump();

        if (this.itemType === 'coin') {
            spawnBouncingCoin(this.mesh.position.x, this.mesh.position.y + 1.2, this.mesh.position.z);
            addCoins(1);
            addScore(100);
        } else if (this.itemType === 'flower') {
            spawnPowerup('flower', this.mesh.position.x, this.mesh.position.y + 1.2, this.mesh.position.z);
        } else if (this.itemType === 'mega') {
            spawnPowerup('mega', this.mesh.position.x, this.mesh.position.y + 1.2, this.mesh.position.z);
        }
    }

    update(dt) {
        if (this.bounceProgress > 0) {
            this.bounceProgress -= dt * 6;
            if (this.bounceProgress < 0) this.bounceProgress = 0;
            const bounceOffset = Math.sin(this.bounceProgress * Math.PI) * 0.45;
            this.mesh.position.y = this.initialY + bounceOffset;
            this.box.setFromObject(this.mesh);
        }
    }
}

// Brick Block class
class BrickBlock {
    constructor(x, y, z) {
        const geo = new THREE.BoxGeometry(1.6, 1.6, 1.6);
        this.mesh = new THREE.Mesh(geo, materials.brick);
        this.mesh.position.set(x, y, z);
        this.mesh.castShadow = true;
        this.mesh.receiveShadow = true;
        scene.add(this.mesh);

        this.box = new THREE.Box3().setFromObject(this.mesh);
        blocks.push(this);
    }

    shatter() {
        window.soundEngine.playBrickBreak();
        scene.remove(this.mesh);
        const idx = blocks.indexOf(this);
        if (idx > -1) blocks.splice(idx, 1);

        // Spawn 4 debris chunks
        for (let i = 0; i < 6; i++) {
            const chunkGeo = new THREE.BoxGeometry(0.5, 0.5, 0.5);
            const chunk = new THREE.Mesh(chunkGeo, materials.brick);
            chunk.position.copy(this.mesh.position);
            scene.add(chunk);

            const vel = new THREE.Vector3(
                (Math.random() - 0.5) * 8,
                Math.random() * 8 + 4,
                (Math.random() - 0.5) * 8
            );
            particles.push({ mesh: chunk, vel, life: 1.2 });
        }
        addScore(50);
    }
}

// Spawn Question & Brick Blocks in rows
new QuestionBlock(-2, 4, 8, 'coin');
new BrickBlock(0, 4, 8);
new QuestionBlock(2, 4, 8, 'flower');

new QuestionBlock(-3, 8, 22, 'coin');
new BrickBlock(-1, 8, 22);
new QuestionBlock(1, 8, 22, 'mega');
new BrickBlock(3, 8, 22);

// Collectible 3D Gold Coins
class CoinItem {
    constructor(x, y, z) {
        const geo = new THREE.CylinderGeometry(0.5, 0.5, 0.12, 16);
        geo.rotateX(Math.PI / 2);
        this.mesh = new THREE.Mesh(geo, materials.gold);
        this.mesh.position.set(x, y, z);
        this.mesh.castShadow = true;
        scene.add(this.mesh);
        this.collected = false;
        coins.push(this);
    }

    update(dt) {
        if (this.collected) return;
        this.mesh.rotation.y += dt * 3.5;

        // Check collision with Mario
        if (mario.root.position.distanceTo(this.mesh.position) < 1.4) {
            this.collect();
        }
    }

    collect() {
        if (this.collected) return;
        this.collected = true;
        window.soundEngine.playCoin();
        scene.remove(this.mesh);
        const idx = coins.indexOf(this);
        if (idx > -1) coins.splice(idx, 1);

        addCoins(1);
        addScore(100);
        showToast('+100');
    }
}

// Scatter coins across the course
for (let z = 5; z <= 60; z += 6) {
    new CoinItem(Math.sin(z) * 4, 1.2, z);
}
new CoinItem(0, 6.2, 18);
new CoinItem(-5, 10.2, 30);
new CoinItem(5, 14.2, 42);

// Bonus coin pop from hit block
function spawnBouncingCoin(x, y, z) {
    const geo = new THREE.CylinderGeometry(0.5, 0.5, 0.12, 16);
    geo.rotateX(Math.PI / 2);
    const mesh = new THREE.Mesh(geo, materials.gold);
    mesh.position.set(x, y, z);
    scene.add(mesh);

    window.soundEngine.playCoin();
    showToast('+100');

    let vy = 9;
    const interval = setInterval(() => {
        mesh.position.y += vy * 0.03;
        mesh.rotation.y += 0.35;
        vy -= 22 * 0.03;
        if (vy < -6) {
            clearInterval(interval);
            scene.remove(mesh);
        }
    }, 30);
}

// Powerup Spawner (Fire Flower & Mega Mushroom)
function spawnPowerup(type, x, y, z) {
    window.soundEngine.playPowerup();
    const group = new THREE.Group();
    group.position.set(x, y, z);

    if (type === 'flower') {
        const stem = new THREE.Mesh(new THREE.CylinderGeometry(0.08, 0.08, 0.4), materials.koopaGreen);
        stem.position.y = 0.2;
        group.add(stem);

        const flower = new THREE.Mesh(new THREE.TorusGeometry(0.28, 0.12, 8, 16), materials.marioRed);
        flower.position.y = 0.5;
        group.add(flower);

        const center = new THREE.Mesh(new THREE.SphereGeometry(0.18), materials.marioYellow);
        center.position.y = 0.5;
        group.add(center);
    } else if (type === 'mega') {
        const stem = new THREE.Mesh(new THREE.CylinderGeometry(0.3, 0.35, 0.4), materials.marioWhite);
        stem.position.y = 0.2;
        group.add(stem);

        const cap = new THREE.Mesh(new THREE.SphereGeometry(0.55, 16, 16, 0, Math.PI * 2, 0, Math.PI * 0.55), materials.marioYellow);
        cap.position.y = 0.35;
        group.add(cap);
    }

    scene.add(group);

    const checkOverlap = setInterval(() => {
        group.rotation.y += 0.05;
        if (mario.root.position.distanceTo(group.position) < 1.8) {
            clearInterval(checkOverlap);
            scene.remove(group);

            if (type === 'flower') {
                setFireMario();
            } else if (type === 'mega') {
                setMegaMario();
            }
        }
    }, 40);
}

function setFireMario() {
    state.powerup = 'fire';
    mario.shirt.material = materials.marioWhite;
    mario.cap.material = materials.marioWhite;
    mario.brim.material = materials.marioWhite;
    document.getElementById('powerup-badge').style.display = 'block';
    document.getElementById('powerup-badge').innerText = '🔥 FIRE MARIO';
    showToast('FIRE POWER!');
}

function setMegaMario() {
    state.powerup = 'mega';
    state.megaTimer = 15;
    window.soundEngine.playPowerup();
    document.getElementById('powerup-badge').style.display = 'block';
    document.getElementById('powerup-badge').innerText = '🍄 MEGA MARIO';
    showToast('MEGA MARIO!');
}

function resetPowerup() {
    state.powerup = 'normal';
    mario.shirt.material = materials.marioRed;
    mario.cap.material = materials.marioRed;
    mario.brim.material = materials.marioRed;
    mario.bodyContainer.scale.set(1, 1, 1);
    document.getElementById('powerup-badge').style.display = 'none';
}

// --- WARP PIPE ---
function createWarpPipe(x, y, z, height = 3, destY = null) {
    const pipeGroup = new THREE.Group();
    pipeGroup.position.set(x, y, z);

    const bodyGeo = new THREE.CylinderGeometry(1.4, 1.4, height, 16);
    const body = new THREE.Mesh(bodyGeo, materials.pipeGreen);
    body.position.y = height * 0.5;
    body.castShadow = true;
    body.receiveShadow = true;
    pipeGroup.add(body);

    const rimGeo = new THREE.CylinderGeometry(1.65, 1.65, 0.6, 16);
    const rim = new THREE.Mesh(rimGeo, materials.pipeGreen);
    rim.position.y = height;
    rim.castShadow = true;
    pipeGroup.add(rim);

    scene.add(pipeGroup);

    const box = new THREE.Box3().setFromObject(pipeGroup);
    platforms.push({ mesh: pipeGroup, box, w: 3, h: height, d: 3 });

    return { pipeGroup, topY: y + height, destY };
}

const surfacePipe = createWarpPipe(8, 0, 15, 3, -23.5);
const cavePipe = createWarpPipe(8, -25, 25, 3, 1.5);

// --- GOOMBA ENEMY CLASS ---
class Goomba {
    constructor(x, y, z, patrolRadius = 6) {
        this.group = new THREE.Group();
        this.group.position.set(x, y, z);

        // Body mushroom cap
        const bodyGeo = new THREE.SphereGeometry(0.7, 12, 12, 0, Math.PI * 2, 0, Math.PI * 0.65);
        const body = new THREE.Mesh(bodyGeo, materials.goombaBrown);
        body.position.y = 0.5;
        body.castShadow = true;
        this.group.add(body);

        // Face & Eyes
        const eyeGeo = new THREE.SphereGeometry(0.12, 8, 8);
        const eyeL = new THREE.Mesh(eyeGeo, materials.marioWhite);
        eyeL.position.set(-0.25, 0.45, 0.6);
        this.group.add(eyeL);

        const eyeR = eyeL.clone();
        eyeR.position.x = 0.25;
        this.group.add(eyeR);

        // Feet
        const footGeo = new THREE.SphereGeometry(0.25, 8, 8);
        this.footL = new THREE.Mesh(footGeo, materials.goombaFeet);
        this.footL.position.set(-0.35, 0.15, 0);
        this.group.add(this.footL);

        this.footR = this.footL.clone();
        this.footR.position.x = 0.35;
        this.group.add(this.footR);

        scene.add(this.group);

        this.startX = x;
        this.patrolRadius = patrolRadius;
        this.dir = 1;
        this.speed = 2.5;
        this.dead = false;

        enemies.push(this);
    }

    update(dt) {
        if (this.dead) return;

        // Feet waddle
        this.footL.position.y = 0.15 + Math.sin(Date.now() * 0.01) * 0.08;
        this.footR.position.y = 0.15 - Math.sin(Date.now() * 0.01) * 0.08;

        // Player detection AI
        const distToMario = this.group.position.distanceTo(mario.root.position);
        if (distToMario < 12) {
            // Chase Mario
            const toMario = new THREE.Vector3().subVectors(mario.root.position, this.group.position);
            toMario.y = 0;
            toMario.normalize();
            this.group.position.addScaledVector(toMario, this.speed * 1.3 * dt);
            this.group.rotation.y = Math.atan2(toMario.x, toMario.z);
        } else {
            // Patrol back and forth
            this.group.position.x += this.dir * this.speed * dt;
            if (Math.abs(this.group.position.x - this.startX) > this.patrolRadius) {
                this.dir *= -1;
                this.group.rotation.y = this.dir > 0 ? 0 : Math.PI;
            }
        }

        // Mario collision check
        if (distToMario < 1.4) {
            if (state.powerup === 'mega') {
                this.die();
            } else if (player.vel.y < -1.5 && mario.root.position.y > this.group.position.y + 0.4) {
                // Mario stomped Goomba
                player.vel.y = player.jumpForce * 0.75;
                this.die();
            } else if (state.invincibleTimer <= 0) {
                // Goomba hurt Mario
                marioTakeDamage();
            }
        }
    }

    die() {
        if (this.dead) return;
        this.dead = true;
        window.soundEngine.playStomp();
        addScore(200);
        showToast('+200');

        // Squash animation
        this.group.scale.set(1.4, 0.2, 1.4);
        spawnBouncingCoin(this.group.position.x, this.group.position.y + 0.5, this.group.position.z);

        setTimeout(() => {
            scene.remove(this.group);
            const idx = enemies.indexOf(this);
            if (idx > -1) enemies.splice(idx, 1);
        }, 500);
    }
}

new Goomba(-4, 0, 16, 5);
new Goomba(3, 0, 28, 4);
new Goomba(0, 0, 48, 6);

// --- KOOPA TROOPA & SHELL CLASS ---
class Koopa {
    constructor(x, y, z) {
        this.group = new THREE.Group();
        this.group.position.set(x, y, z);

        // Shell
        const shellGeo = new THREE.SphereGeometry(0.65, 12, 12);
        shellGeo.scale(1, 1.2, 0.85);
        this.shell = new THREE.Mesh(shellGeo, materials.koopaGreen);
        this.shell.position.y = 0.7;
        this.shell.castShadow = true;
        this.group.add(this.shell);

        // Head
        const headGeo = new THREE.SphereGeometry(0.35, 10, 10);
        const head = new THREE.Mesh(headGeo, materials.koopaYellow);
        head.position.set(0, 1.3, 0.3);
        this.group.add(head);

        scene.add(this.group);
        this.speed = 2.2;
        this.dir = 1;
        this.startX = x;
        this.dead = false;

        enemies.push(this);
    }

    update(dt) {
        if (this.dead) return;

        this.group.position.x += this.dir * this.speed * dt;
        if (Math.abs(this.group.position.x - this.startX) > 4) {
            this.dir *= -1;
            this.group.rotation.y = this.dir > 0 ? 0 : Math.PI;
        }

        const dist = this.group.position.distanceTo(mario.root.position);
        if (dist < 1.5) {
            if (player.vel.y < -1.5 && mario.root.position.y > this.group.position.y + 0.5) {
                // Stomp Koopa into Shell
                player.vel.y = player.jumpForce * 0.75;
                this.retractToShell();
            } else if (state.invincibleTimer <= 0) {
                marioTakeDamage();
            }
        }
    }

    retractToShell() {
        this.dead = true;
        window.soundEngine.playStomp();
        scene.remove(this.group);
        const idx = enemies.indexOf(this);
        if (idx > -1) enemies.splice(idx, 1);

        new KoopaShell(this.group.position.x, 0.5, this.group.position.z);
    }
}

new Koopa(2, 0, 22);

// Koopa Shell Class
class KoopaShell {
    constructor(x, y, z) {
        const geo = new THREE.SphereGeometry(0.65, 12, 12);
        geo.scale(1, 0.9, 1);
        this.mesh = new THREE.Mesh(geo, materials.koopaGreen);
        this.mesh.position.set(x, y, z);
        this.mesh.castShadow = true;
        scene.add(this.mesh);

        this.vel = new THREE.Vector3();
        this.isMoving = false;
        shells.push(this);
    }

    kick(dir) {
        window.soundEngine.playKick();
        this.vel.copy(dir).multiplyScalar(26);
        this.isMoving = true;
    }

    update(dt) {
        if (!this.isMoving) {
            // Check if Mario kicks it
            if (mario.root.position.distanceTo(this.mesh.position) < 1.4) {
                const kickDir = new THREE.Vector3().subVectors(this.mesh.position, mario.root.position).normalize();
                kickDir.y = 0;
                this.kick(kickDir);
            }
            return;
        }

        this.mesh.rotation.x += dt * 25;
        this.mesh.position.addScaledVector(this.vel, dt);

        // Bounce off level walls
        if (Math.abs(this.mesh.position.x) > 11) {
            this.vel.x *= -1;
            window.soundEngine.playKick();
        }

        // Break bricks or defeat Goombas
        enemies.forEach(e => {
            if (!e.dead && this.mesh.position.distanceTo(e.group.position) < 1.4) {
                e.die();
            }
        });
    }
}

// --- FLAGPOLE GOAL ---
function createFlagpole(x, y, z) {
    const poleGroup = new THREE.Group();
    poleGroup.position.set(x, y, z);

    // Stone base
    const baseGeo = new THREE.BoxGeometry(2.5, 1, 2.5);
    const base = new THREE.Mesh(baseGeo, materials.dirt);
    base.position.y = 0.5;
    poleGroup.add(base);

    // Pole
    const poleGeo = new THREE.CylinderGeometry(0.12, 0.12, 12, 12);
    const pole = new THREE.Mesh(poleGeo, materials.marioWhite);
    pole.position.y = 6.5;
    poleGroup.add(pole);

    // Golden Ball top
    const ballGeo = new THREE.SphereGeometry(0.4, 12, 12);
    const ball = new THREE.Mesh(ballGeo, materials.gold);
    ball.position.y = 12.6;
    poleGroup.add(ball);

    // Flag
    const flagGeo = new THREE.PlaneGeometry(2, 1.4);
    const flagMat = new THREE.MeshStandardMaterial({ color: 0x22c55e, side: THREE.DoubleSide });
    const flag = new THREE.Mesh(flagGeo, flagMat);
    flag.position.set(1.1, 11.5, 0);
    poleGroup.add(flag);

    scene.add(poleGroup);

    return { poleGroup, poleX: x, poleZ: z, flag };
}

flagpoleObj = createFlagpole(0, 0, 72);

// --- FIREBALL PROJECTILE ---
function shootFireball() {
    if (state.powerup !== 'fire') return;

    window.soundEngine.playFireball();

    const geo = new THREE.SphereGeometry(0.28, 8, 8);
    const mat = new THREE.MeshStandardMaterial({ color: 0xf97316, emissive: 0xea580c, emissiveIntensity: 0.8 });
    const fbMesh = new THREE.Mesh(geo, mat);

    const forward = new THREE.Vector3(0, 0, 1).applyAxisAngle(new THREE.Vector3(0, 1, 0), player.facingAngle);
    fbMesh.position.copy(mario.root.position).add(new THREE.Vector3(0, 0.8, 0)).addScaledVector(forward, 0.6);
    scene.add(fbMesh);

    const fb = {
        mesh: fbMesh,
        vel: forward.multiplyScalar(22),
        vy: -2,
        bounces: 0,
        life: 3.5
    };
    fireballs.push(fb);
}

// --- INPUT HANDLING ---
const keys = {};
let mouseDrag = false;
let prevMouseX = 0;
let cameraOrbitYaw = 0;
let cameraOrbitPitch = 0.35;

window.addEventListener('keydown', (e) => {
    keys[e.code] = true;

    if (e.code === 'Space') {
        player.jumpBufferTimer = 0.16;
        e.preventDefault();
    }
    if (e.code === 'KeyV') {
        handleActionV();
    }
    if (e.code === 'Escape' || e.code === 'KeyP') {
        togglePause();
    }
});

window.addEventListener('keyup', (e) => {
    keys[e.code] = false;
});

window.addEventListener('mousedown', (e) => {
    if (e.target.closest('#ui-layer') && !e.target.classList.contains('touch-btn')) return;

    if (e.button === 0) {
        mouseDrag = true;
        prevMouseX = e.clientX;
        if (state.powerup === 'fire') {
            shootFireball();
        }
    } else if (e.button === 2) {
        // Kick / Throw shell if near
        shells.forEach(s => {
            if (mario.root.position.distanceTo(s.mesh.position) < 2.5) {
                const forward = new THREE.Vector3(0, 0, 1).applyAxisAngle(new THREE.Vector3(0, 1, 0), player.facingAngle);
                s.kick(forward);
            }
        });
    }
});

window.addEventListener('contextmenu', e => e.preventDefault());

window.addEventListener('mouseup', () => {
    mouseDrag = false;
});

window.addEventListener('mousemove', (e) => {
    if (mouseDrag) {
        const dx = e.clientX - prevMouseX;
        cameraOrbitYaw -= dx * 0.005;
        prevMouseX = e.clientX;
    }
});

function handleActionV() {
    // Pipe entry check
    const dSurface = mario.root.position.distanceTo(new THREE.Vector3(surfacePipe.pipeGroup.position.x, surfacePipe.topY, surfacePipe.pipeGroup.position.z));
    const dCave = mario.root.position.distanceTo(new THREE.Vector3(cavePipe.pipeGroup.position.x, cavePipe.topY, cavePipe.pipeGroup.position.z));

    if (dSurface < 2.2) {
        window.soundEngine.playPipe();
        mario.root.position.set(cavePipe.pipeGroup.position.x, cavePipe.destY, cavePipe.pipeGroup.position.z);
        player.vel.set(0, 0, 0);
        return;
    } else if (dCave < 2.2) {
        window.soundEngine.playPipe();
        mario.root.position.set(surfacePipe.pipeGroup.position.x, surfacePipe.destY, surfacePipe.pipeGroup.position.z);
        player.vel.set(0, 0, 0);
        return;
    }

    // Otherwise Ground Pound if airborne
    if (!player.grounded && !player.isGroundPounding) {
        player.isGroundPounding = true;
        player.groundPoundState = 'pause';
        player.vel.set(0, 0, 0);
        window.soundEngine.playJump(2);

        setTimeout(() => {
            if (player.isGroundPounding) {
                player.groundPoundState = 'fall';
                player.vel.y = -35;
            }
        }, 250);
    }
}

// Touch control bindings
document.getElementById('btn-jump')?.addEventListener('touchstart', (e) => { e.preventDefault(); player.jumpBufferTimer = 0.16; });
document.getElementById('btn-action')?.addEventListener('touchstart', (e) => { e.preventDefault(); shootFireball(); });
document.getElementById('btn-pound')?.addEventListener('touchstart', (e) => { e.preventDefault(); handleActionV(); });

// HUD Button Bindings
document.getElementById('btn-pause')?.addEventListener('click', togglePause);
document.getElementById('btn-resume')?.addEventListener('click', togglePause);
document.getElementById('btn-restart')?.addEventListener('click', () => restartLevel());
document.getElementById('btn-retry')?.addEventListener('click', () => restartLevel());
document.getElementById('btn-next')?.addEventListener('click', () => restartLevel());

document.getElementById('btn-sound')?.addEventListener('click', () => {
    window.soundEngine.resume();
    window.soundEngine.startBGM('overworld');
    showToast('Sound Enabled 🎵');
});

// --- DAMAGE & HEALTH SYSTEM ---
function marioTakeDamage() {
    if (state.invincibleTimer > 0 || state.gameOver) return;

    if (state.powerup !== 'normal') {
        resetPowerup();
        window.soundEngine.playHurt();
        state.invincibleTimer = 2.0;
    } else {
        state.lives--;
        updateHUD();
        window.soundEngine.playHurt();

        if (state.lives <= 0) {
            triggerGameOver();
        } else {
            state.invincibleTimer = 2.0;
            player.vel.y = 8;
        }
    }
}

function triggerGameOver() {
    state.gameOver = true;
    document.getElementById('gameover-modal').style.display = 'flex';
}

function triggerCourseClear() {
    if (state.courseClear) return;
    state.courseClear = true;
    window.soundEngine.playFlagpole();

    // Flag slides down
    const flagInterval = setInterval(() => {
        if (flagpoleObj && flagpoleObj.flag.position.y > 1.5) {
            flagpoleObj.flag.position.y -= 0.15;
        } else {
            clearInterval(flagInterval);
        }
    }, 20);

    setTimeout(() => {
        document.getElementById('clear-stats').innerHTML = `
            <div>Score: <b>${state.score}</b></div>
            <div>Coins Collected: <b>${state.coins}</b></div>
            <div>Time Remaining: <b>${Math.max(0, Math.floor(state.time))}s</b></div>
        `;
        document.getElementById('clear-modal').style.display = 'flex';
    }, 2200);
}

function restartLevel() {
    state.lives = 5;
    state.coins = 0;
    state.score = 0;
    state.time = 300;
    state.isPaused = false;
    state.gameOver = false;
    state.courseClear = false;
    resetPowerup();

    mario.root.position.set(0, 0, 0);
    player.vel.set(0, 0, 0);
    if (flagpoleObj) flagpoleObj.flag.position.y = 11.5;

    document.getElementById('pause-modal').style.display = 'none';
    document.getElementById('clear-modal').style.display = 'none';
    document.getElementById('gameover-modal').style.display = 'none';
    updateHUD();
}

function togglePause() {
    if (state.gameOver || state.courseClear) return;
    state.isPaused = !state.isPaused;
    document.getElementById('pause-modal').style.display = state.isPaused ? 'flex' : 'none';
}

function addCoins(amount) {
    state.coins += amount;
    if (state.coins >= 100) {
        state.coins = 0;
        state.lives++;
        window.soundEngine.play1Up();
        showToast('1-UP! 🍄');
    }
    updateHUD();
}

function addScore(amount) {
    state.score += amount;
    updateHUD();
}

function updateHUD() {
    document.getElementById('lives-val').innerText = state.lives;
    document.getElementById('coins-val').innerText = state.coins.toString().padStart(2, '0');
    document.getElementById('score-val').innerText = state.score.toString().padStart(5, '0');
    document.getElementById('time-val').innerText = Math.max(0, Math.floor(state.time));
}

function showToast(text) {
    const el = document.getElementById('toast-message');
    if (!el) return;
    el.innerText = text;
    el.style.opacity = '1';
    el.style.transform = 'translate(-50%, -65%) scale(1.2)';

    setTimeout(() => {
        el.style.opacity = '0';
        el.style.transform = 'translate(-50%, -50%) scale(1)';
    }, 800);
}

// --- MAIN PHYSICS & GAME LOOP ---
const clock = new THREE.Clock();

function animate() {
    requestAnimationFrame(animate);

    const dt = Math.min(clock.getDelta(), 0.05);

    if (state.isPaused || state.gameOver) {
        renderer.render(scene, camera);
        return;
    }

    // Stage timer
    if (!state.courseClear) {
        state.time -= dt;
        if (state.time <= 0) {
            marioTakeDamage();
            state.time = 300;
        }
        updateHUD();
    }

    // Mega Mario countdown
    if (state.powerup === 'mega') {
        state.megaTimer -= dt;
        mario.bodyContainer.scale.lerp(new THREE.Vector3(2.4, 2.4, 2.4), 0.1);
        if (state.megaTimer <= 0) resetPowerup();
    } else {
        mario.bodyContainer.scale.lerp(new THREE.Vector3(1, 1, 1), 0.15);
    }

    // Invincibility flicker
    if (state.invincibleTimer > 0) {
        state.invincibleTimer -= dt;
        mario.bodyContainer.visible = Math.floor(Date.now() / 80) % 2 === 0;
    } else {
        mario.bodyContainer.visible = true;
    }

    // Player Input Movement Vector
    let moveX = 0;
    let moveZ = 0;

    if (keys['KeyW'] || keys['ArrowUp']) moveZ += 1;
    if (keys['KeyS'] || keys['ArrowDown']) moveZ -= 1;
    if (keys['KeyA'] || keys['ArrowLeft']) moveX -= 1;
    if (keys['KeyD'] || keys['ArrowRight']) moveX += 1;

    player.isCrouching = !!(keys['ShiftLeft'] || keys['ShiftRight']);

    // Grounding & Coyote timer
    if (player.grounded) {
        player.coyoteTimer = 0.12;
    } else {
        player.coyoteTimer -= dt;
    }
    player.jumpBufferTimer -= dt;

    // Jumping with coyote & buffer
    if (player.jumpBufferTimer > 0 && player.coyoteTimer > 0 && !player.isGroundPounding) {
        player.jumpBufferTimer = 0;
        player.coyoteTimer = 0;
        player.grounded = false;

        const now = clock.getElapsedTime();
        if (now - player.lastJumpTime < 0.9) {
            player.jumpCount = Math.min(3, player.jumpCount + 1);
        } else {
            player.jumpCount = 1;
        }
        player.lastJumpTime = now;

        const bonus = player.jumpCount === 3 ? 1.3 : (player.jumpCount === 2 ? 1.15 : 1.0);
        player.vel.y = player.jumpForce * bonus;
        window.soundEngine.playJump(player.jumpCount);
    }

    // Movement calculation
    if (!player.isGroundPounding) {
        const inputDir = new THREE.Vector3(moveX, 0, moveZ);
        if (inputDir.lengthSq() > 0.01) {
            inputDir.normalize();

            // Rotate input relative to camera orbit
            inputDir.applyAxisAngle(new THREE.Vector3(0, 1, 0), cameraOrbitYaw);

            const curSpeed = player.isCrouching ? player.speed * 0.4 : player.speed;
            player.vel.x = THREE.MathUtils.lerp(player.vel.x, inputDir.x * curSpeed, 0.2);
            player.vel.z = THREE.MathUtils.lerp(player.vel.z, inputDir.z * curSpeed, 0.2);

            player.facingAngle = Math.atan2(inputDir.x, inputDir.z);
            mario.root.rotation.y = THREE.MathUtils.lerp(mario.root.rotation.y, player.facingAngle, 0.25);

            // Animate running limbs
            player.walkAnimPhase += dt * (curSpeed * 1.5);
            mario.leftLeg.rotation.x = Math.sin(player.walkAnimPhase) * 0.7;
            mario.rightLeg.rotation.x = -Math.sin(player.walkAnimPhase) * 0.7;
            mario.leftArm.rotation.x = -Math.sin(player.walkAnimPhase) * 0.7;
            mario.rightArm.rotation.x = Math.sin(player.walkAnimPhase) * 0.7;
        } else {
            player.vel.x = THREE.MathUtils.lerp(player.vel.x, 0, 0.25);
            player.vel.z = THREE.MathUtils.lerp(player.vel.z, 0, 0.25);

            mario.leftLeg.rotation.x = THREE.MathUtils.lerp(mario.leftLeg.rotation.x, 0, 0.2);
            mario.rightLeg.rotation.x = THREE.MathUtils.lerp(mario.rightLeg.rotation.x, 0, 0.2);
            mario.leftArm.rotation.x = THREE.MathUtils.lerp(mario.leftArm.rotation.x, 0, 0.2);
            mario.rightArm.rotation.x = THREE.MathUtils.lerp(mario.rightArm.rotation.x, 0, 0.2);
        }
    }

    // Apply gravity
    if (!player.grounded && player.groundPoundState !== 'pause') {
        player.vel.y -= 42 * dt;
    }

    // Move player
    player.pos.x += player.vel.x * dt;
    player.pos.z += player.vel.z * dt;
    player.pos.y += player.vel.y * dt;

    // Platform & Floor Collisions
    player.grounded = false;

    platforms.forEach(p => {
        const marioBox = new THREE.Box3().setFromCenterAndSize(
            new THREE.Vector3(player.pos.x, player.pos.y + 0.9, player.pos.z),
            new THREE.Vector3(0.7, 1.8, 0.7)
        );

        if (marioBox.intersectsBox(p.box)) {
            // Landing on top
            if (player.vel.y <= 0 && player.pos.y >= p.box.max.y - 0.4) {
                player.pos.y = p.box.max.y;
                player.vel.y = 0;
                player.grounded = true;
                if (player.isGroundPounding) {
                    player.isGroundPounding = false;
                    player.groundPoundState = 'none';
                    window.soundEngine.playBlockBump();
                }
            }
        }
    });

    // Block collision (Question & Bricks)
    blocks.forEach(b => {
        const marioBox = new THREE.Box3().setFromCenterAndSize(
            new THREE.Vector3(player.pos.x, player.pos.y + 0.9, player.pos.z),
            new THREE.Vector3(0.7, 1.8, 0.7)
        );

        if (marioBox.intersectsBox(b.box)) {
            // Hit from underneath
            if (player.vel.y > 0 && player.pos.y < b.box.min.y) {
                player.pos.y = b.box.min.y - 1.8;
                player.vel.y = -2;
                b.hit ? b.hit() : b.shatter();
            }
            // Land on top
            else if (player.vel.y <= 0 && player.pos.y >= b.box.max.y - 0.3) {
                player.pos.y = b.box.max.y;
                player.vel.y = 0;
                player.grounded = true;

                if (player.isGroundPounding && b.shatter) {
                    b.shatter();
                }
            }
        }
    });

    // Flagpole collision trigger
    if (flagpoleObj && !state.courseClear) {
        const dPole = new THREE.Vector2(player.pos.x - flagpoleObj.poleX, player.pos.z - flagpoleObj.poleZ).length();
        if (dPole < 1.4 && player.pos.y > 0) {
            triggerCourseClear();
        }
    }

    // Pit fall check
    if (player.pos.y < -35) {
        marioTakeDamage();
        player.pos.set(0, 2, 0);
        player.vel.set(0, 0, 0);
    }

    // Update Question Blocks
    blocks.forEach(b => { if (b.update) b.update(dt); });

    // Update Coins
    coins.forEach(c => c.update(dt));

    // Update Enemies
    enemies.forEach(e => e.update(dt));

    // Update Shells
    shells.forEach(s => s.update(dt));

    // Update Fireballs
    for (let i = fireballs.length - 1; i >= 0; i--) {
        const fb = fireballs[i];
        fb.life -= dt;
        fb.vy -= 35 * dt;

        fb.mesh.position.addScaledVector(fb.vel, dt);
        fb.mesh.position.y += fb.vy * dt;

        // Ground bounce
        if (fb.mesh.position.y <= 0.3) {
            fb.mesh.position.y = 0.3;
            fb.vy = 8;
            fb.bounces++;
        }

        // Hit enemies
        enemies.forEach(e => {
            if (!e.dead && fb.mesh.position.distanceTo(e.group.position) < 1.2) {
                e.die();
                fb.life = 0;
            }
        });

        if (fb.life <= 0 || fb.bounces > 4) {
            scene.remove(fb.mesh);
            fireballs.splice(i, 1);
        }
    }

    // Update Particles
    for (let i = particles.length - 1; i >= 0; i--) {
        const p = particles[i];
        p.life -= dt;
        p.vel.y -= 25 * dt;
        p.mesh.position.addScaledVector(p.vel, dt);
        p.mesh.rotation.x += dt * 5;

        if (p.life <= 0) {
            scene.remove(p.mesh);
            particles.splice(i, 1);
        }
    }

    // --- SMOOTH CAMERA FOLLOW & ORBIT ---
    const camDist = 11;
    const camHeight = 5.5;

    const targetCamX = player.pos.x - Math.sin(cameraOrbitYaw) * camDist;
    const targetCamZ = player.pos.z - Math.cos(cameraOrbitYaw) * camDist;
    const targetCamY = player.pos.y + camHeight;

    camera.position.x = THREE.MathUtils.lerp(camera.position.x, targetCamX, 0.1);
    camera.position.y = THREE.MathUtils.lerp(camera.position.y, targetCamY, 0.1);
    camera.position.z = THREE.MathUtils.lerp(camera.position.z, targetCamZ, 0.1);

    const lookTarget = new THREE.Vector3(player.pos.x, player.pos.y + 1.2, player.pos.z);
    camera.lookAt(lookTarget);

    renderer.render(scene, camera);
}

// Window resize
window.addEventListener('resize', () => {
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(window.innerWidth, window.innerHeight);
});

// Start loop
animate();
