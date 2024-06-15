function animateBackgroundCanvas(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) {
        console.error(`Canvas with id ${canvasId} not found.`);
        return;
    }
    const ctx = canvas.getContext('2d');

    // Configuration variables
    const griderWidth = 10; // Width of the griders and their trails
    const trailRelativeLength = 1; // Relative length of the trail to the screen size
    const griderSpeed = 0.5; // Speed of the griders
    const griderDensity = 4 / (1024 * 768); // Density of griders per pixel
    const griderColors = ['#FFA500', '#00FFFF']; // Orange and Cyan colors
    
    // Set the canvas to full screen size
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;

    const griders = [];

    // Griders move in random directions.
    class Grider {
        constructor(x, y, width, color) {
            this.x = x;
            this.y = y;
            this.width = width;
            this.color = color;

            const directions = ['up', 'down', 'left', 'right'];
            this.direction = directions[Math.floor(Math.random() * directions.length)];

            this.turns = 0;
            this.maxTurns = Math.random() * (0.75 - 0.1) + 0.1;
            this.trail = [];
        }

        draw() {
            // Draw the gridder
            ctx.fillStyle = this.color;
            ctx.fillRect(this.x, this.y, this.width, this.width);

            // Draw the trail
            this.trail.forEach(segment => {
                ctx.fillRect(segment.x, segment.y, this.width, this.width);
            });
        }

        move() {
            // Calculate the distance of each move
            const moveDistance = griderSpeed * this.width;
            // Move the grider and add the position to the trail
            switch (this.direction) {
                case 'up':
                    this.y -= moveDistance;
                    break;
                case 'down':
                    this.y += moveDistance;
                    break;
                case 'left':
                    this.x -= moveDistance;
                    break;
                case 'right':
                    this.x += moveDistance;
                    break;
            }

            // Add the current position to the trail
            this.trail.push({ x: this.x, y: this.y });

            // Keep the trail at the configured length
            const trailLength = Math.round(trailRelativeLength * ((canvas.width + canvas.height) / 2))
            while (this.trail.length > trailLength / this.width) {
                this.trail.shift();
            }

            // Handle turning
            this.turns += moveDistance;
            if (this.turns >= this.maxTurns * canvas.width) {
                this.turn();
                this.turns = 0;
                this.maxTurns = Math.random() * (0.75 - 0.1) + 0.1;
            }

            // Keep the grider on the canvas
            this.keepOnCanvas();
        }

        turn() {
            // Randomly turn left or right at 90 degree angles
            const turnLeft = Math.random() < 0.5;
            switch (this.direction) {
                case 'up':
                    this.direction = turnLeft ? 'left' : 'right';
                    break;
                case 'down':
                    this.direction = turnLeft ? 'right' : 'left';
                    break;
                case 'left':
                    this.direction = turnLeft ? 'down' : 'up';
                    break;
                case 'right':
                    this.direction = turnLeft ? 'up' : 'down';
                    break;
            }
        }

        keepOnCanvas() {
            // If the grider is about to move off the canvas, turn it around
            if (this.x < 0) {
                this.x = 0;
                this.direction = 'right';
            } else if (this.x + this.width > canvas.width) {
                this.x = canvas.width - this.width;
                this.direction = 'left';
            }
            if (this.y < 0) {
                this.y = 0;
                this.direction = 'down';
            } else if (this.y + this.width > canvas.height) {
                this.y = canvas.height - this.width;
                this.direction = 'up';
            }
        }
    }
    
    // Animate the background. Called every frame.
    function animate() {

        // Clear the canvas
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        // Move and draw each grider
        griders.forEach(grider => {
            grider.move();
            grider.draw();
        });

        // Call this function again on the next animation frame
        requestAnimationFrame(animate);
    }

    // Adjust canvas size when the window is resized
    window.addEventListener('resize', resizeCanvas, false);
    
    function resizeCanvas() {
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
        
        // Recalculate the number of griders for the new canvas size
        const newNumberOfGriders = Math.round(griderDensity * canvas.width * canvas.height);
        // Add or remove griders to match the new density
        while (griders.length < newNumberOfGriders) {
            const x = Math.random() * (canvas.width - griderWidth);
            const y = Math.random() * (canvas.height - griderWidth);
            const color = griderColors[griders.length % griderColors.length];
            griders.push(new Grider(x, y, griderWidth, color));
        }
        while (griders.length > newNumberOfGriders) {
            griders.pop();
        }
    }

    // Start the animation
    resizeCanvas();
    animate();
}
