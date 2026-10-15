"""Local-only UDP relay for one development client; never use for public hosting.

Example: python udp_latency.py --report report.json
Host listens on 7780, client connects to 127.0.0.1:7779.
"""
import argparse
import heapq
import json
import random
import select
import socket
import time
from pathlib import Path


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--report', type=Path, required=True)
    parser.add_argument('--seconds', type=float, default=180)
    parser.add_argument('--delay-ms', type=float, default=75)
    parser.add_argument('--jitter-ms', type=float, default=20)
    parser.add_argument('--loss', type=float, default=0.02)
    args = parser.parse_args()
    if not (0 <= args.loss < 1 and args.delay_ms >= 0 and args.jitter_ms >= 0 and args.seconds > 0):
        parser.error('Invalid simulation parameters')
    rng = random.Random(12345)
    stats = {'received': 0, 'forwarded': 0, 'dropped': 0, 'bytes': 0,
             'delayMsPerDirection': args.delay_ms, 'jitterMs': args.jitter_ms,
             'lossProbability': args.loss, 'seed': 12345}
    queue = []
    peer = None
    host = ('127.0.0.1', 7780)
    args.report.parent.mkdir(parents=True, exist_ok=True)
    start = last_report = time.monotonic()
    with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as wire:
        wire.bind(('127.0.0.1', 7779))
        wire.setblocking(False)
        while time.monotonic() - start < args.seconds:
            now = time.monotonic()
            while queue and queue[0][0] <= now:
                _, _, payload, target = heapq.heappop(queue)
                wire.sendto(payload, target)
                stats['forwarded'] += 1
            if now - last_report >= 1:
                stats['elapsedSeconds'] = now - start
                args.report.write_text(json.dumps(stats, indent=2), encoding='utf-8')
                last_report = now
            if not select.select([wire], [], [], 0.005)[0]:
                continue
            payload, source = wire.recvfrom(65535)
            if source != host:
                if peer is None:
                    peer = source
                if source != peer:
                    continue
            target = peer if source == host else host
            if target is None:
                continue
            stats['received'] += 1
            stats['bytes'] += len(payload)
            if rng.random() < args.loss:
                stats['dropped'] += 1
                continue
            delay = max(0, args.delay_ms + rng.uniform(-args.jitter_ms, args.jitter_ms)) / 1000
            heapq.heappush(queue, (now + delay, stats['received'], payload, target))
        stats['elapsedSeconds'] = time.monotonic() - start
        stats['pendingAtStop'] = len(queue)
        args.report.write_text(json.dumps(stats, indent=2), encoding='utf-8')


if __name__ == '__main__':
    main()
