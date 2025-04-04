import { useState, useEffect } from 'react';

const AudioVisualizer = () => {
    const [bars, setBars] = useState([
        { height: 6, animationDuration: '1.2s', animationDelay: '0s' },
        { height: 8, animationDuration: '1.5s', animationDelay: '0.2s' },
        { height: 5, animationDuration: '1.3s', animationDelay: '0.1s' },
    ]);

    useEffect(() => {
        // Use a faster interval for more dynamic updates
        const interval = setInterval(() => {
            setBars(() => [
                {
                    height: 4 + Math.random() * 8,
                    animationDuration: `${1.2 + Math.random() * 0.5}s`,
                    animationDelay: `${Math.random() * 0.3}s`,
                },
                {
                    height: 4 + Math.random() * 8,
                    animationDuration: `${1.2 + Math.random() * 0.5}s`,
                    animationDelay: `${Math.random() * 0.3}s`,
                },
                {
                    height: 4 + Math.random() * 8,
                    animationDuration: `${1.2 + Math.random() * 0.5}s`,
                    animationDelay: `${Math.random() * 0.3}s`,
                }
            ]);
        }, 1000); // Update every 1 second for a more dynamic effect

        return () => {
            clearInterval(interval);
        };
    }, []);

    return (
        <div className="flex justify-center items-end space-x-1 h-6">
            {bars.map((bar, index) => (
                <div
                    key={index}
                    style={{
                        height: `${bar.height}px`,
                        width: '4px', // Wider bars
                        backgroundColor: '#4F46E5', // Indigo color for the bars
                        animation: `equalizer ${bar.animationDuration} ease-in-out infinite alternate`,
                        animationDelay: bar.animationDelay,
                        borderRadius: '1px',
                    }}
                />
            ))}
            <style>{`
        @keyframes equalizer {
          0% {
            height: 3px;
          }
          100% {
            height: 16px;
          }
        }
      `}</style>
        </div>
    );
};

export default AudioVisualizer;